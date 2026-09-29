using System.Net;
using Microsoft.EntityFrameworkCore;
using OW3N.Tests.TestSupport;
using Xunit;

namespace OW3N.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="PlayerCharacterService"/>: character/gauge/roll persistence and
/// the external roll-service call (stubbed via <see cref="StubHttpMessageHandler"/>).
/// </summary>
public class PlayerCharacterServiceTests
{
    private static PlayerCharacterService NewService(
        IDbContextFactory<OrdisContext> factory, HttpClient? http = null, LiveUpdateService? live = null)
        => new(factory, http ?? new HttpClient(StubHttpMessageHandler.WithResponse(HttpStatusCode.NoContent)),
               live ?? new LiveUpdateService());

    [Fact]
    public async Task CreateAsync_then_GetByIdAsync_round_trips_a_character()
    {
        using var factory = new SqliteContextFactory();
        var service = NewService(factory);

        var character = new PlayerCharacter { Name = "Aria", UserId = "u1", Gauges = new List<Gauge>(), Rolls = new List<RollResult>() };
        await service.CreateAsync(character);

        var loaded = await service.GetByIdAsync(character.Id);
        Assert.NotNull(loaded);
        Assert.Equal("Aria", loaded!.Name);
    }

    [Fact]
    public async Task UpdateGaugeAsync_adds_a_new_gauge_then_updates_it_in_place()
    {
        using var factory = new SqliteContextFactory();
        var character = await Seed.CharacterAsync(factory, userId: "u1");
        var live = new LiveUpdateService();
        int notifications = 0;
        live.OnCharacterChanged += _ => notifications++;
        var service = NewService(factory, live: live);

        var gauge = new Gauge { Id = Guid.NewGuid(), Name = "health", Max = 20, Value = 20, PlayerCharacterId = character.Id };
        await service.UpdateGaugeAsync(gauge);

        gauge.Value = 5;
        gauge.Name = "hp";
        await service.UpdateGaugeAsync(gauge);

        await using var db = await factory.CreateDbContextAsync();
        var stored = Assert.Single(await db.Gauges.ToListAsync());
        Assert.Equal("hp", stored.Name);
        Assert.Equal(5, stored.Value);
        Assert.Equal(2, notifications);
    }

    [Fact]
    public async Task SaveRollAsync_appends_a_roll_and_stamps_a_timestamp()
    {
        using var factory = new SqliteContextFactory();
        var character = await Seed.CharacterAsync(factory, userId: "u1");
        var service = NewService(factory);

        await service.SaveRollAsync(character.Id, new RollResult { Result = 7, Message = "hit", Rolls = new List<IndividualRollResult>() });

        await using var db = await factory.CreateDbContextAsync();
        var stored = await db.Characters.Include(c => c.Rolls).SingleAsync();
        var roll = Assert.Single(stored.Rolls!);
        Assert.Equal(7, roll.Result);
        Assert.NotNull(roll.Timestamp);
    }

    [Fact]
    public async Task SaveRollAsync_throws_for_unknown_character()
    {
        using var factory = new SqliteContextFactory();
        var service = NewService(factory);

        await Assert.ThrowsAnyAsync<Exception>(
            () => service.SaveRollAsync(12345, new RollResult { Rolls = new List<IndividualRollResult>() }));
    }

    [Fact]
    public async Task GetLatestRollAsync_returns_the_earliest_timestamped_roll()
    {
        // NOTE: despite its name, the implementation orders ascending by Timestamp and takes the
        // first, so it returns the EARLIEST roll. This test pins the current behaviour; if the
        // method is meant to return the most recent roll, the ordering is a bug (see ow3n-77 notes).
        using var factory = new SqliteContextFactory();
        var character = await Seed.CharacterAsync(factory, userId: "u1");
        var service = NewService(factory);

        var older = new RollResult { Result = 1, Message = "old", Timestamp = new DateTime(2020, 1, 1), Rolls = new List<IndividualRollResult>() };
        var newer = new RollResult { Result = 2, Message = "new", Timestamp = new DateTime(2024, 1, 1), Rolls = new List<IndividualRollResult>() };
        await service.SaveRollAsync(character.Id, newer);
        await service.SaveRollAsync(character.Id, older);

        var result = await service.GetLatestRollAsync(character);

        Assert.NotNull(result);
        Assert.Equal("old", result!.Message);
    }

    [Fact]
    public async Task GetByDiscordIdAsync_returns_only_that_users_characters()
    {
        using var factory = new SqliteContextFactory();
        await Seed.CharacterAsync(factory, userId: "u1", name: "Mine1");
        await Seed.CharacterAsync(factory, userId: "u1", name: "Mine2");
        await Seed.CharacterAsync(factory, userId: "u2", name: "Theirs");
        var service = NewService(factory);

        var mine = (await service.GetByDiscordIdAsync("u1")).Select(c => c.Name).ToList();

        Assert.Equal(2, mine.Count);
        Assert.DoesNotContain("Theirs", mine);
    }

    // ---- RollFor: external roll service ----

    [Fact]
    public async Task RollFor_posts_to_the_roll_service_and_saves_the_result()
    {
        using var factory = new SqliteContextFactory();
        var character = await Seed.CharacterAsync(factory, userId: "u1");
        var handler = StubHttpMessageHandler.WithResponse(
            HttpStatusCode.OK,
            """{ "result": 9, "message": "crit", "rolls": [ { "result": 9, "expression": "1d20" } ] }""");
        var service = NewService(factory, new HttpClient(handler));

        var result = await service.RollFor(character, "1d20");

        Assert.Equal(9f, result.Result);
        Assert.Equal("crit", result.Message);

        // Request went to the documented roll-service endpoint.
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"http://localhost:3000/roll/{character.Id}/1d20", request.RequestUri!.ToString());

        // ...and the roll was persisted.
        await using var db = await factory.CreateDbContextAsync();
        var stored = await db.Characters.Include(c => c.Rolls).SingleAsync();
        Assert.Single(stored.Rolls!);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task RollFor_throws_InvalidRollException_for_error_statuses(HttpStatusCode status)
    {
        using var factory = new SqliteContextFactory();
        var character = await Seed.CharacterAsync(factory, userId: "u1");
        var service = NewService(factory, new HttpClient(StubHttpMessageHandler.WithResponse(status)));

        await Assert.ThrowsAsync<InvalidRollException>(() => service.RollFor(character, "1d20"));
    }

    [Fact]
    public async Task RollFor_throws_InvalidRollException_for_unparseable_body()
    {
        using var factory = new SqliteContextFactory();
        var character = await Seed.CharacterAsync(factory, userId: "u1");
        var service = NewService(factory, new HttpClient(StubHttpMessageHandler.WithResponse(HttpStatusCode.OK, "not json")));

        await Assert.ThrowsAsync<InvalidRollException>(() => service.RollFor(character, "1d20"));
    }
}
