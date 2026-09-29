using Microsoft.EntityFrameworkCore;
using OW3N.Tests.TestSupport;
using Xunit;

namespace OW3N.Tests.Integration;

/// <summary>
/// Integration tests for <see cref="BuffService"/>: template libraries, campaign export/import,
/// pull-from-source, and applying buffs to characters.
/// </summary>
public class BuffServiceTests
{
    private static BuffService NewService(IDbContextFactory<OrdisContext> factory, LiveUpdateService? live = null)
        => new(factory, live ?? new LiveUpdateService());

    // ---- Libraries ----

    [Fact]
    public async Task GetPlayerLibraryAsync_returns_only_the_owners_templates_ordered_by_name()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "owner");
        await Seed.UserAsync(factory, "other");
        await Seed.TemplateAsync(factory, name: "Zephyr", ownerId: "owner");
        await Seed.TemplateAsync(factory, name: "Aegis", ownerId: "owner");
        await Seed.TemplateAsync(factory, name: "NotMine", ownerId: "other");
        var service = NewService(factory);

        var library = await service.GetPlayerLibraryAsync("owner");

        Assert.Equal(new[] { "Aegis", "Zephyr" }, library.Select(t => t.Name));
    }

    [Fact]
    public async Task SearchTemplatesAsync_returns_owner_and_campaign_templates()
    {
        // NOTE: the name-filtering branch uses EF.Functions.ILike, which is PostgreSQL-only and is
        // therefore not exercised here (see OW3N.Tests/README.md for the SQLite-vs-PostgreSQL
        // rationale). With an empty query it is skipped, so we assert the owner/campaign scoping
        // that does run on any provider.
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "owner");
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm");
        await Seed.TemplateAsync(factory, name: "Mine", ownerId: "owner");
        await Seed.TemplateAsync(factory, name: "Campaign", campaignId: campaign.Id);
        await Seed.UserAsync(factory, "stranger");
        await Seed.TemplateAsync(factory, name: "Theirs", ownerId: "stranger");
        var service = NewService(factory);

        var results = await service.SearchTemplatesAsync("", "owner", campaign.Id);

        var names = results.Select(t => t.Name).ToList();
        Assert.Contains("Mine", names);
        Assert.Contains("Campaign", names);
        Assert.DoesNotContain("Theirs", names);
    }

    // ---- CRUD ----

    [Fact]
    public async Task CreateTemplateAsync_persists_and_notifies_for_campaign_templates()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm");
        var live = new LiveUpdateService();
        int? notified = null;
        live.OnCampaignChanged += id => notified = id;
        var service = NewService(factory, live);

        await service.CreateTemplateAsync(new BuffTemplate { Name = "Aura", CampaignId = campaign.Id });

        await using var db = await factory.CreateDbContextAsync();
        Assert.Equal("Aura", (await db.BuffTemplates.SingleAsync()).Name);
        Assert.Equal(campaign.Id, notified);
    }

    [Fact]
    public async Task UpdateTemplateAsync_updates_fields_and_is_a_noop_for_missing_templates()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "owner");
        var template = await Seed.TemplateAsync(factory, name: "Old", ownerId: "owner");
        var service = NewService(factory);

        template.Name = "New";
        template.Icon = "flame";
        await service.UpdateTemplateAsync(template);

        // Should not throw for a non-existent id.
        await service.UpdateTemplateAsync(new BuffTemplate { Id = Guid.NewGuid(), Name = "ghost" });

        await using var db = await factory.CreateDbContextAsync();
        var stored = await db.BuffTemplates.SingleAsync();
        Assert.Equal("New", stored.Name);
        Assert.Equal("flame", stored.Icon);
    }

    [Fact]
    public async Task DeleteTemplateAsync_removes_the_template()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "owner");
        var template = await Seed.TemplateAsync(factory, ownerId: "owner");
        var service = NewService(factory);

        await service.DeleteTemplateAsync(template.Id);

        await using var db = await factory.CreateDbContextAsync();
        Assert.Empty(await db.BuffTemplates.ToListAsync());
    }

    // ---- Export / import / pull ----

    [Fact]
    public async Task ExportToCampaignAsync_copies_the_template_and_links_the_source()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "owner");
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm");
        var source = await Seed.TemplateAsync(factory, name: "Blessing", ownerId: "owner");
        var service = NewService(factory);

        var campaignTemplate = await service.ExportToCampaignAsync(source.Id, campaign.Id);

        Assert.NotEqual(source.Id, campaignTemplate.Id);
        Assert.Equal(campaign.Id, campaignTemplate.CampaignId);
        Assert.Equal("Blessing", campaignTemplate.Name);

        await using var db = await factory.CreateDbContextAsync();
        var reloadedSource = await db.BuffTemplates.SingleAsync(t => t.Id == source.Id);
        Assert.Equal(campaignTemplate.Id, reloadedSource.SourceTemplateId);
    }

    [Fact]
    public async Task ExportToCampaignAsync_throws_when_source_missing()
    {
        using var factory = new SqliteContextFactory();
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm");
        var service = NewService(factory);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExportToCampaignAsync(Guid.NewGuid(), campaign.Id));
    }

    [Fact]
    public async Task ImportFromCampaignAsync_creates_a_linked_player_copy_and_is_idempotent()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "player");
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm");
        var campaignTemplate = await Seed.TemplateAsync(factory, name: "Ward", campaignId: campaign.Id);
        var service = NewService(factory);

        var first = await service.ImportFromCampaignAsync(campaignTemplate.Id, "player");
        var second = await service.ImportFromCampaignAsync(campaignTemplate.Id, "player");

        Assert.Equal(first.Id, second.Id); // idempotent
        Assert.Equal("player", first.OwnerId);
        Assert.Equal(campaignTemplate.Id, first.SourceTemplateId);

        await using var db = await factory.CreateDbContextAsync();
        Assert.Single(await db.BuffTemplates.Where(t => t.OwnerId == "player").ToListAsync());
    }

    [Fact]
    public async Task PullFromSourceAsync_resets_the_template_to_match_its_source()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "player");
        var campaign = await Seed.CampaignAsync(factory, dmId: "dm");
        var source = await Seed.TemplateAsync(factory, name: "Canonical", campaignId: campaign.Id);
        var playerCopy = await Seed.TemplateAsync(factory, name: "Drifted", ownerId: "player", sourceTemplateId: source.Id);
        var service = NewService(factory);

        await service.PullFromSourceAsync(playerCopy.Id);

        await using var db = await factory.CreateDbContextAsync();
        var stored = await db.BuffTemplates.SingleAsync(t => t.Id == playerCopy.Id);
        Assert.Equal("Canonical", stored.Name);
    }

    // ---- Applying buffs to characters ----

    [Fact]
    public async Task ApplyBuffAsync_attaches_the_buff_and_dedupes_per_template()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "owner");
        var character = await Seed.CharacterAsync(factory, userId: "owner");
        var template = await Seed.TemplateAsync(factory, ownerId: "owner");
        var live = new LiveUpdateService();
        int? notified = null;
        live.OnCharacterChanged += id => notified = id;
        var service = NewService(factory, live);

        var first = await service.ApplyBuffAsync(character.Id, template.Id);
        var second = await service.ApplyBuffAsync(character.Id, template.Id);

        Assert.Equal(first.Id, second.Id); // no duplicate row
        Assert.Equal(character.Id, notified);

        await using var db = await factory.CreateDbContextAsync();
        Assert.Single(await db.CharacterBuffs.ToListAsync());
    }

    [Fact]
    public async Task RemoveBuffAsync_detaches_the_buff()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "owner");
        var character = await Seed.CharacterAsync(factory, userId: "owner");
        var template = await Seed.TemplateAsync(factory, ownerId: "owner");
        var service = NewService(factory);
        var buff = await service.ApplyBuffAsync(character.Id, template.Id);

        await service.RemoveBuffAsync(buff.Id);

        await using var db = await factory.CreateDbContextAsync();
        Assert.Empty(await db.CharacterBuffs.ToListAsync());
    }

    [Fact]
    public async Task UpdateStacksAsync_clamps_negative_values_to_zero()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "owner");
        var character = await Seed.CharacterAsync(factory, userId: "owner");
        var template = await Seed.TemplateAsync(factory, ownerId: "owner");
        var service = NewService(factory);
        var buff = await service.ApplyBuffAsync(character.Id, template.Id);

        await service.UpdateStacksAsync(buff.Id, 4);
        Assert.Equal(4, (await GetBuff(factory, buff.Id)).Stacks);

        await service.UpdateStacksAsync(buff.Id, -10);
        Assert.Equal(0, (await GetBuff(factory, buff.Id)).Stacks);
    }

    [Fact]
    public async Task GetCharacterBuffsAsync_returns_buffs_with_templates_loaded()
    {
        using var factory = new SqliteContextFactory();
        await Seed.UserAsync(factory, "owner");
        var character = await Seed.CharacterAsync(factory, userId: "owner");
        var template = await Seed.TemplateAsync(factory, name: "Focus", ownerId: "owner");
        var service = NewService(factory);
        await service.ApplyBuffAsync(character.Id, template.Id);

        var buffs = await service.GetCharacterBuffsAsync(character.Id);

        var buff = Assert.Single(buffs);
        Assert.NotNull(buff.Template);
        Assert.Equal("Focus", buff.Template.Name);
    }

    private static async Task<CharacterBuff> GetBuff(IDbContextFactory<OrdisContext> factory, Guid id)
    {
        await using var db = await factory.CreateDbContextAsync();
        return await db.CharacterBuffs.SingleAsync(b => b.Id == id);
    }
}
