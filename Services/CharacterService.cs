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
        Console.WriteLine("1");
        var db = await dbFactory.CreateDbContextAsync();

        Console.WriteLine("2");
        db.Characters.Update(c);

        Console.WriteLine("3");
        await db.SaveChangesAsync();

        Console.WriteLine("4");
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

        return await db.Characters.Include(g => g.Gauges).SingleOrDefaultAsync(c => c.Id == id);
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
