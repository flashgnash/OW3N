using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

public class PlayerCharacter
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? Name { get; set; }

    public Campaign? Campaign { get; set; }

    public string? RollServerId { get; set; }

    [NotMapped]
    public IEnumerable<Item> Inventory { get; set; } =
        new List<Item>()
        {
            // new Item() {
            //     Icon = "🍔",
            //     Name = "Cheeseburger"
            // },
            // new Item() {
            //     Icon = "⚔️",
            //     Name = "Sword",
            //     Rolls = new List<Roll>() {new Roll(){Name = "attack", RollString="1d12+1"} }
            // }
        };

    [NotMapped]
    public IEnumerable<Spell> Spells { get; set; } =
        new List<Spell>()
        {
            // new Spell() {
            //     Icon = "🧊",
            //     Name = "Ice Bolt",
            //     Rolls = new List<Roll>() {new Roll(){Name = "attack", RollString="1d12+1"} }

            // },
            // new Spell() {
            //     Icon = "🔥",
            //     Name = "Fireball",
            //     Rolls = new List<Roll>() {new Roll(){Name = "attack", RollString="1d12+1"} }
            // }
        };

    [NotMapped]
    public string? Race { get; set; }

    [NotMapped]
    public List<Stat>? Stats =>
        StatBlock?.Stats?
            .Where(x => x.Value != 0)
            .Select(x => new Stat { Name = x.Key, Value = x.Value })
            .ToList();

    [NotMapped]
    public List<Stat>? SpecialStats =>
        StatBlock?.SpecialStats?
            .Where(x => x.Value != 0)
            .Select(x => new Stat { Name = x.Key, Value = x.Value })
            .ToList();

    [NotMapped]
    public IEnumerable<Status>? Statuses { get; set; }

    public ICollection<Gauge>? Gauges { get; set; }

    // public string? StatBlockHash { get; set; }
    [Column("stat_block")]
    public string? StatBlockJson { get; set; }

    [NotMapped]
    public StatBlock? StatBlock
    {
        get => string.IsNullOrEmpty(StatBlockJson)
            ? null
            : JsonSerializer.Deserialize<StatBlock>(StatBlockJson);

        set => StatBlockJson = value == null
            ? null
            : JsonSerializer.Serialize(value);
    }
    

    public string? StatBlockHash { get; set; }
    public string? StatBlockMessageId { get; set; }
    public string? StatBlockChannelId { get; set; }
    public string? StatBlockServerId { get; set; }

    public string? SpellBlockChannelId { get; set; }
    public string? SpellBlockMessageId { get; set; }
    public string? SpellBlock { get; set; }
    public string? SpellBlockHash { get; set; }
    public int? Mana { get; set; }
    public string? ManaReadoutChannelId { get; set; }
    public string? ManaReadoutMessageId { get; set; }

    public string? SavedRolls { get; set; }

    [NotMapped]
    public Dictionary<string, string>? SavedRollsDict
    {
        get
        {
            var saved_rolls = SavedRolls; //this is kind of atrocious but I am limited by discord's UI (and my own laziness)

            return saved_rolls
                ?.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                ?.Select(line => line.Split(':', 2))
                ?.ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
        }
    }

    // [Column("stats")]
    // JsonDocument? _statJson;

    // private JsonDocument? StatJson
    // {
    //     get
    //     {
    //         if (_statJson == null && !string.IsNullOrEmpty(StatBlock))
    //         {
    //             try { _statJson = JsonDocument.Parse(StatBlock); }
    //             catch { _statJson = null; }
    //         }
    //         return _statJson;
    //     }
    // }



    // public string? StatBlockServerId { get; set; }
}
