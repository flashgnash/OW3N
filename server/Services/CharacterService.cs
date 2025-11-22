using Npgsql;

public class PlayerCharacterService : IDbService<PlayerCharacter, int>
{
    readonly string _connStr;
    public PlayerCharacterService(string connStr) => _connStr = connStr;

    public void Insert(PlayerCharacter c)
    {

        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            INSERT INTO characters (
                user_id, name, stat_block_hash, stat_block, stat_block_message_id, stat_block_channel_id,
                spell_block_channel_id, spell_block_message_id, spell_block, spell_block_hash, mana,
                mana_readout_channel_id, mana_readout_message_id, saved_rolls, roll_server_id
            ) VALUES (
                @user_id, @name, @stat_block_hash, @stat_block, @stat_block_message_id, @stat_block_channel_id,
                @spell_block_channel_id, @spell_block_message_id, @spell_block, @spell_block_hash, @mana,
                @mana_readout_channel_id, @mana_readout_message_id, @saved_rolls, @roll_server_id
            )", conn);
        AddParams(cmd, c);
        cmd.ExecuteNonQuery();
    }

    public void Update(PlayerCharacter c)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand(@"
            UPDATE characters SET
                user_id=@user_id, name=@name, stat_block_hash=@stat_block_hash, stat_block=@stat_block,
                stat_block_message_id=@stat_block_message_id, stat_block_channel_id=@stat_block_channel_id,
                spell_block_channel_id=@spell_block_channel_id, spell_block_message_id=@spell_block_message_id,
                spell_block=@spell_block, spell_block_hash=@spell_block_hash, mana=@mana,
                mana_readout_channel_id=@mana_readout_channel_id, mana_readout_message_id=@mana_readout_message_id,
                saved_rolls=@saved_rolls, roll_server_id=@roll_server_id
            WHERE id=@id", conn);
        AddParams(cmd, c);
        cmd.Parameters.AddWithValue("@id", c.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("DELETE FROM characters WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public PlayerCharacter? GetById(int id)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT * FROM characters WHERE id=@id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;
        return ReadPlayerCharacter(r);
    }

    public IEnumerable<PlayerCharacter> GetByDiscordId(string discordId)
    {
        using var conn = new NpgsqlConnection(_connStr);
        conn.Open();
        using var cmd = new NpgsqlCommand("SELECT * FROM characters WHERE user_id=@id", conn);
        cmd.Parameters.AddWithValue("@id", discordId);
        using var r = cmd.ExecuteReader();

        var list = new List<PlayerCharacter>();
        while (r.Read()) list.Add(ReadPlayerCharacter(r));
        return list;
    }

    void AddParams(NpgsqlCommand cmd, PlayerCharacter c)
    {
        cmd.Parameters.AddWithValue("@user_id", (object?)c.UserId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@name", (object?)c.Name ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@saved_rolls", (object?)c.SavedRollsString ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stat_block_hash", (object?)c.StatBlockHash ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stat_block", (object?)c.StatBlock ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stat_block_message_id", (object?)c.StatBlockMessageId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@stat_block_channel_id", (object?)c.StatBlockChannelId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spell_block_channel_id", (object?)c.SpellBlockChannelId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spell_block_message_id", (object?)c.SpellBlockMessageId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spell_block", (object?)c.SpellBlock ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@spell_block_hash", (object?)c.SpellBlockHash ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@mana", (object?)c.Mana ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@mana_readout_channel_id", (object?)c.ManaReadoutChannelId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@mana_readout_message_id", (object?)c.ManaReadoutMessageId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@roll_server_id", (object?)c.RollServerId ?? DBNull.Value);
    }

    PlayerCharacter ReadPlayerCharacter(NpgsqlDataReader r) => new PlayerCharacter
    {
        Id = r.GetInt32(r.GetOrdinal("id")),
        UserId = r["user_id"] as string,
        Name = r["name"] as string,
        RollServerId = r["roll_server_id"] as string,
        StatBlockHash = r["stat_block_hash"] as string,
        SavedRollsString = r["saved_rolls"] as string,
        StatBlock = r["stat_block"] as string,
        StatBlockMessageId = r["stat_block_message_id"] as string,
        StatBlockChannelId = r["stat_block_channel_id"] as string,
        SpellBlockChannelId = r["spell_block_channel_id"] as string,
        SpellBlockMessageId = r["spell_block_message_id"] as string,
        SpellBlock = r["spell_block"] as string,
        SpellBlockHash = r["spell_block_hash"] as string,
        Mana = r["mana"] as int?,
        ManaReadoutChannelId = r["mana_readout_channel_id"] as string,
        ManaReadoutMessageId = r["mana_readout_message_id"] as string,
    };
}
