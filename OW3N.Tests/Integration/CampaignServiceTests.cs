using Microsoft.EntityFrameworkCore;
using OW3N.Tests.TestSupport;
using Xunit;

namespace OW3N.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="CampaignService"/> against a real (SQLite) database, covering
/// the DM authorisation gate, player/NPC/preset management and preset-based character creation.
/// </summary>
public class CampaignServiceTests
{
    private static CampaignService NewService(IDbContextFactory<OrdisContext> factory)
        => new(factory, new LiveUpdateService());

    // ---- Authorisation ----

    [Fact]
    public async Task AddPlayerToCampaignAsync_lets_the_dm_add_a_player()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        var service = NewService(factory);

        var player = await service.AddPlayerToCampaignAsync(campaign.Id, "dm1");

        Assert.False(player.IsNpc);
        Assert.Equal("New Player", player.Name);

        await using var db = await factory.CreateDbContextAsync();
        var stored = await db.Campaigns.Include(c => c.Players).SingleAsync();
        Assert.Single(stored.Players!);
    }

    [Fact]
    public async Task AddPlayerToCampaignAsync_rejects_a_non_dm_caller()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        var service = NewService(factory);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.AddPlayerToCampaignAsync(campaign.Id, "intruder"));
    }

    [Fact]
    public async Task AddPlayerToCampaignAsync_rejects_a_missing_caller_identity()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        var service = NewService(factory);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.AddPlayerToCampaignAsync(campaign.Id, ""));
    }

    [Fact]
    public async Task AddPlayerToCampaignAsync_throws_for_unknown_campaign()
    {
        using var factory = new SqliteContextFactory();
        var service = NewService(factory);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddPlayerToCampaignAsync(999, "dm1"));
    }

    [Fact]
    public async Task AddNpcAsync_marks_the_character_as_an_npc()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        var service = NewService(factory);

        var npc = await service.AddNpcAsync(campaign.Id, "dm1");

        Assert.True(npc.IsNpc);
        Assert.Equal("New NPC", npc.Name);
    }

    // ---- Create / update ----

    [Fact]
    public async Task UpdateAsync_creating_a_campaign_assigns_dm_and_default_preset()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "dm1");
        var service = NewService(factory);

        var campaign = new Campaign { Name = "Fresh" };
        await service.UpdateAsync(campaign, "dm1");

        await using var db = await factory.CreateDbContextAsync();
        var stored = await db.Campaigns.Include(c => c.Presets).SingleAsync();
        Assert.Equal("dm1", stored.DungeonMasterId);
        var preset = Assert.Single(stored.Presets!);
        Assert.Equal("Character", preset.Name);
        Assert.Equal(6, preset.StatBlock!.Stats!.Count);
        Assert.Contains(preset.Gauges, g => g.Name == "health" && g.Max == 100);
    }

    [Fact]
    public async Task UpdateAsync_rejects_a_non_dm_editing_an_existing_campaign()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1", name: "Original");
        var service = NewService(factory);

        var edit = new Campaign { Id = campaign.Id, Name = "Hijacked", DungeonMasterId = "intruder" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateAsync(edit, "intruder"));
    }

    [Fact]
    public async Task UpdateAsync_ignores_client_supplied_dm_id_on_update()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1", name: "Original");
        var service = NewService(factory);

        var edit = new Campaign { Id = campaign.Id, Name = "Renamed", DungeonMasterId = "attacker" };
        await service.UpdateAsync(edit, "dm1");

        await using var db = await factory.CreateDbContextAsync();
        var stored = await db.Campaigns.SingleAsync();
        Assert.Equal("Renamed", stored.Name);
        Assert.Equal("dm1", stored.DungeonMasterId); // not "attacker"
    }

    [Fact]
    public void CreateDefaultPreset_produces_six_base_stats_and_a_health_gauge()
    {
        var preset = CampaignService.CreateDefaultPreset(campaignId: 5);

        Assert.Equal(5, preset.CampaignId);
        Assert.Equal(6, preset.StatBlock!.Stats!.Count);
        Assert.All(preset.StatBlock.Stats!, s => Assert.Equal(10, s.Value));
        var gauge = Assert.Single(preset.Gauges);
        Assert.Equal("health", gauge.Name);
        Assert.Equal(GaugeType.IconBar, gauge.GaugeType);
    }

    // ---- Create characters from preset ----

    [Fact]
    public async Task CreateCharactersFromPresetAsync_creates_numbered_characters_with_full_gauges()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        int presetId = await AddPreset(factory, campaign.Id, name: "Goblin", gaugeMax: 12);
        var service = NewService(factory);

        var created = await service.CreateCharactersFromPresetAsync(campaign.Id, presetId, isNpc: true, count: 2, "dm1");

        Assert.Equal(new[] { "Goblin 1", "Goblin 2" }, created.Select(c => c.Name));
        Assert.All(created, c =>
        {
            Assert.True(c.IsNpc);
            var gauge = Assert.Single(c.Gauges!);
            Assert.Equal(12, gauge.Max);
            Assert.Equal(12, gauge.Value); // gauges start full
        });
    }

    [Fact]
    public async Task CreateCharactersFromPresetAsync_skips_already_used_numbers()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        int presetId = await AddPreset(factory, campaign.Id, name: "Goblin", gaugeMax: 10);

        // Pre-existing "Goblin 2" should be skipped by the numbering.
        await using (var db = await factory.CreateDbContextAsync())
        {
            var c = await db.Campaigns.Include(x => x.Players).SingleAsync();
            c.Players!.Add(new PlayerCharacter { Name = "Goblin 2" });
            await db.SaveChangesAsync();
        }

        var service = NewService(factory);
        var created = await service.CreateCharactersFromPresetAsync(campaign.Id, presetId, isNpc: true, count: 2, "dm1");

        Assert.Equal(new[] { "Goblin 1", "Goblin 3" }, created.Select(c => c.Name));
    }

    [Fact]
    public async Task CreateCharactersFromPresetAsync_returns_empty_for_unknown_preset()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        var service = NewService(factory);

        var created = await service.CreateCharactersFromPresetAsync(campaign.Id, presetId: 424242, isNpc: false, count: 1, "dm1");

        Assert.Empty(created);
    }

    [Fact]
    public async Task CreateCharactersFromPresetAsync_requires_dm()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        int presetId = await AddPreset(factory, campaign.Id, name: "Goblin", gaugeMax: 10);
        var service = NewService(factory);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateCharactersFromPresetAsync(campaign.Id, presetId, false, 1, "intruder"));
    }

    // ---- Remove player / preset / campaign ----

    [Fact]
    public async Task RemovePlayerAsync_removes_the_player()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        var service = NewService(factory);
        var player = await service.AddPlayerToCampaignAsync(campaign.Id, "dm1");

        await service.RemovePlayerAsync(campaign, player, "dm1");

        await using var db = await factory.CreateDbContextAsync();
        var stored = await db.Campaigns.Include(c => c.Players).SingleAsync();
        Assert.Empty(stored.Players!);
    }

    [Fact]
    public async Task RemovePresetAsync_rejects_a_preset_from_another_campaign()
    {
        using var factory = new SqliteContextFactory();
        var campaignA = await Seed.CampaignAsync(factory, dmId: "dm1", name: "A");
        var campaignB = await Seed.CampaignAsync(factory, dmId: "dm1", name: "B");
        int presetInB = await AddPreset(factory, campaignB.Id, name: "P", gaugeMax: 10);
        var service = NewService(factory);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.RemovePresetAsync(campaignA.Id, presetInB, "dm1"));
    }

    [Fact]
    public async Task DeleteAsync_removes_the_campaign()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        var service = NewService(factory);

        await service.DeleteAsync(campaign.Id, "dm1");

        await using var db = await factory.CreateDbContextAsync();
        Assert.Empty(await db.Campaigns.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_requires_dm()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm1");
        var service = NewService(factory);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteAsync(campaign.Id, "intruder"));
    }

    // ---- Queries ----

    [Fact]
    public async Task GetByDiscordIdAsync_returns_campaigns_where_the_user_is_dm_or_a_player()
    {
        using var factory = new SqliteContextFactory();
        var asDm = await Seed.CampaignAsync(factory, dmId: "alice", name: "Alice DMs");
        var asPlayer = await Seed.CampaignAsync(factory, dmId: "bob", name: "Bob DMs");
        await Seed.CampaignAsync(factory, dmId: "carol", name: "Unrelated");

        await using (var db = await factory.CreateDbContextAsync())
        {
            var c = await db.Campaigns.Include(x => x.Players).SingleAsync(x => x.Id == asPlayer.Id);
            c.Players!.Add(new PlayerCharacter { Name = "Alice's PC", UserId = "alice" });
            await db.SaveChangesAsync();
        }

        var service = NewService(factory);
        var result = (await service.GetByDiscordIdAsync("alice")).Select(c => c.Name).ToList();

        Assert.Contains("Alice DMs", result);
        Assert.Contains("Bob DMs", result);
        Assert.DoesNotContain("Unrelated", result);
    }

    private static async Task<int> AddPreset(
        IDbContextFactory<OrdisContext> factory, int campaignId, string name, int gaugeMax)
    {
        await using var db = await factory.CreateDbContextAsync();
        var preset = new CharacterPreset
        {
            Name = name,
            CampaignId = campaignId,
            StatBlock = new StatBlock { Stats = new(), SpecialStats = new() },
            Gauges = new() { new() { Name = "health", Max = gaugeMax, Colour = "red", GaugeType = GaugeType.IconBar } },
        };
        db.CharacterPresets.Add(preset);
        await db.SaveChangesAsync();
        return preset.Id;
    }
}
