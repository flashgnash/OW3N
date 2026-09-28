public class Campaign
{
    public int Id { get; set; }
    public string Name { get; set; }

    public User? DungeonMaster { get; set; }
    public string? DungeonMasterId {get; set;}

    public ICollection<PlayerCharacter>? Players { get; set; }

    public ICollection<CharacterPreset>? Presets { get; set; }

    public string? DefaultRollDie { get; set; }
    public string? StatModifierFormula { get; set; }

    // Optional Discord webhook override for this campaign. Falls back to the
    // globally configured webhook when null/empty.
    public string? WebhookUrl { get; set; }
}
