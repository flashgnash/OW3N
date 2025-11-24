using Microsoft.EntityFrameworkCore;

public class PlayerCharacterService(IDbContextFactory<OrdisContext> dbFactory)
{


    public async Task UpdateAsync(PlayerCharacter c)
    {

        var db = await dbFactory.CreateDbContextAsync();

        db.Characters.Update(c);
        await db.SaveChangesAsync();
    }

    public async Task<PlayerCharacter?> GetByIdAsync(int id)
    {

        var db = await dbFactory.CreateDbContextAsync();

        return await db.Characters.FindAsync(id);
    }

    public async Task<IEnumerable<PlayerCharacter>> GetByDiscordIdAsync(string discordId)
    {

        var db = await dbFactory.CreateDbContextAsync();

        return await db.Characters
                 .Where(c => c.UserId == discordId)
                 .ToListAsync();
    }
}
