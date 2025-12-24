using Microsoft.EntityFrameworkCore;

public class PlayerCharacterService(IDbContextFactory<OrdisContext> dbFactory)
{
    public async Task UpdateGaugeAsync(Gauge gauge)
    {
        Console.WriteLine($"Update gauge {gauge.Id}");
        using var db = await dbFactory.CreateDbContextAsync();

        var old = await db.Gauges.FirstOrDefaultAsync(x => x.Id == gauge.Id);

        if (old == null)
            db.Gauges.Add(gauge);
        else
        {
            old.Name = gauge.Name;
            old.Value = gauge.Value;
            old.Max = gauge.Max;
            old.Icon = gauge.Icon;
        }

        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PlayerCharacter c)
    {
        var db = await dbFactory.CreateDbContextAsync();

        c.StatBlock = c.StatBlock;

        db.Characters.Update(c);

        await db.SaveChangesAsync();
    }


    public async Task CreateAsync(PlayerCharacter c) {
        
        var db = await dbFactory.CreateDbContextAsync();

        await db.Characters.AddAsync(c);

        await db.SaveChangesAsync();
    }

    // public async Task CreateOrUpdateAsync(Gauge gauge)
    // {
    //     await using var db = await dbFactory.CreateDbContextAsync();

    //     Console.WriteLine($"pre-find {gauge.Id}");

    //     var existing = await db.Gauges.FindAsync(gauge.Id);

    //     Console.WriteLine($"post-find {gauge.Id}");
    //     if (existing != null)
    //     {
    //         existing.Value = gauge.Value;
    //         existing.Max = gauge.Max;
    //         existing.Icon = gauge.Icon;
    //         existing.Name = gauge.Name;
    //     }
    //     else
    //     {
    //         gauge.Id = Guid.NewGuid(); // make new ID
    //         Console.WriteLine(gauge.Id);
    //         await db.Gauges.AddAsync(gauge);
    //     }

    //     await db.SaveChangesAsync();
    // }

    public async Task<PlayerCharacter?> GetByIdAsync(int id)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db.Characters.Include(g => g.Gauges).Include(g => g.Campaign).SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<PlayerCharacter>> GetByDiscordIdAsync(string discordId)
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        return await db
            .Characters.Include(g => g.Gauges)
            .Where(c => c.UserId == discordId)
            .ToListAsync();
    }
}
