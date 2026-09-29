using Microsoft.EntityFrameworkCore;
using OW3N.Tests.TestSupport;
using Xunit;

namespace OW3N.Tests.Integration;

/// <summary>
/// Confirms the EF model materialises against SQLite (via EnsureCreated) and that a character
/// with related gauges/rolls round-trips through the database.
/// </summary>
public class SchemaSmokeTests
{
    [Fact]
    public async Task Can_create_schema_and_round_trip_a_character()
    {
        using var factory = new SqliteContextFactory();

        await using (var db = await factory.CreateDbContextAsync())
        {
            db.Characters.Add(new PlayerCharacter
            {
                Name = "Smoke",
                UserId = "user-1",
                Gauges = new List<Gauge> { new() { Name = "health", Max = 10, Value = 5 } },
                Rolls = new List<RollResult>(),
            });
            await db.SaveChangesAsync();
        }

        await using (var db = await factory.CreateDbContextAsync())
        {
            var loaded = await db.Characters.Include(c => c.Gauges).SingleAsync();
            Assert.Equal("Smoke", loaded.Name);
            Assert.Single(loaded.Gauges!);
            Assert.Equal("health", loaded.Gauges!.First().Name);
        }
    }
}
