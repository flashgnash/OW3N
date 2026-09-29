using Microsoft.EntityFrameworkCore;

namespace OW3N.Tests.TestSupport;

/// <summary>Helpers for inserting rows into a test database via a context factory.</summary>
public static class Seed
{
    public static async Task<User> UserAsync(IDbContextFactory<OrdisContext> factory, string id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var user = new User { Id = id, Username = id };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task<Campaign> CampaignAsync(
        IDbContextFactory<OrdisContext> factory, string dmId, string name = "Campaign")
    {
        await using var db = await factory.CreateDbContextAsync();
        // DungeonMasterId is a (convention) foreign key to Users, so the DM must exist first —
        // mirrors the app, where a user row is created at login before they can DM a campaign.
        if (await db.Users.FindAsync(dmId) == null)
            db.Users.Add(new User { Id = dmId, Username = dmId });
        var campaign = new Campaign { Name = name, DungeonMasterId = dmId };
        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();
        return campaign;
    }

    public static async Task<PlayerCharacter> CharacterAsync(
        IDbContextFactory<OrdisContext> factory, string? userId = null, string name = "Hero", int? campaignId = null)
    {
        await using var db = await factory.CreateDbContextAsync();
        var character = new PlayerCharacter
        {
            Name = name,
            UserId = userId,
            Gauges = new List<Gauge>(),
            Rolls = new List<RollResult>(),
        };
        if (campaignId != null)
            character.Campaign = await db.Campaigns.FindAsync(campaignId);
        db.Characters.Add(character);
        await db.SaveChangesAsync();
        return character;
    }

    public static async Task<BuffTemplate> TemplateAsync(
        IDbContextFactory<OrdisContext> factory,
        string name = "Buff",
        string? ownerId = null,
        int? campaignId = null,
        Guid? sourceTemplateId = null)
    {
        await using var db = await factory.CreateDbContextAsync();
        var template = new BuffTemplate
        {
            Name = name,
            Icon = "shield",
            Type = BuffType.Status,
            OwnerId = ownerId,
            CampaignId = campaignId,
            SourceTemplateId = sourceTemplateId,
            EffectsJson = "[]",
        };
        db.BuffTemplates.Add(template);
        await db.SaveChangesAsync();
        return template;
    }
}
