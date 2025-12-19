using System.Text.Json.Serialization;

public class StatBlock
{
    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("hunger")]
    public int? Hunger { get; set; }

    [JsonPropertyName("default_roll")]
    public string? DefaultRoll { get; set; }

    [JsonPropertyName("modifier_formula")]
    public string? ModifierFormula { get; set; }

    [JsonPropertyName("actions")]
    public int? Actions { get; set; }

    [JsonPropertyName("reactions")]
    public int? Reactions { get; set; }

    [JsonPropertyName("speed")]
    public int? Speed { get; set; }

    [JsonPropertyName("armour")]
    public int? MaxArmour { get; set; }

    [JsonPropertyName("current_armour")]
    public int? CurrentArmour { get; set; }

    [JsonPropertyName("soul")]
    public int? MaxSoul { get; set; }

    [JsonPropertyName("current_soul")]
    public int? CurrentSoul { get; set; }

    [JsonPropertyName("hp")]
    public int? MaxHealth { get; set; }

    [JsonPropertyName("current_hp")]
    public int? CurrentHealth { get; set; }

    [JsonPropertyName("hpr")]
    public int? HealthRegen { get; set; }

    [JsonPropertyName("energy_pool")]
    public int? EnergyPool { get; set; }

    [JsonPropertyName("stats")]
    public Dictionary<string, int>? Stats { get; set; }

    [JsonPropertyName("special_stats")]
    public Dictionary<string, int>? SpecialStats { get; set; }
}
