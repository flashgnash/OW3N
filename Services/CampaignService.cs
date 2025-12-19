using Microsoft.EntityFrameworkCore;

public class CampaignService(IDbContextFactory<OrdisContext> dbFactory)
{

    public async Task UpdateAsync(Campaign c)
    {
        var db = await dbFactory.CreateDbContextAsync();

        db.Campaigns.Update(c);

        await db.SaveChangesAsync();

    }
    public async Task DeleteAsync(int campaignId) {
        var db = await dbFactory.CreateDbContextAsync();

        db.Campaigns.Remove(new Campaign(){Id = campaignId});

        await db.SaveChangesAsync();
        
    }

    public async Task<Campaign?> GetByIdAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Campaigns.Include(g => g.Players).SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Campaign>> GetByDiscordIdAsync(string discordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db
            .Campaigns.Include(g => g.Players)
            .Where(c => c.DungeonMaster.Id == discordId)
            .ToListAsync();
    }
}
