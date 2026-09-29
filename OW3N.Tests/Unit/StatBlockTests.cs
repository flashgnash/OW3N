using System.Text.Json;
using Xunit;

namespace OW3N.Tests.Unit;

/// <summary>
/// Tests for <see cref="StatBlock"/> and <see cref="StatListConverter"/>: stats are persisted as a
/// JSON object ({"str":10}) but surfaced as a List&lt;Stat&gt;, and named properties map to snake_case JSON.
/// </summary>
public class StatBlockTests
{
    [Fact]
    public void StatListConverter_reads_object_into_stat_list_with_null_as_zero()
    {
        var sb = JsonSerializer.Deserialize<StatBlock>("""
        { "stats": { "str": 10, "dex": null } }
        """)!;

        Assert.NotNull(sb.Stats);
        Assert.Equal(10, sb.Stats!.Single(s => s.Name == "str").Value);
        Assert.Equal(0, sb.Stats!.Single(s => s.Name == "dex").Value);
    }

    [Fact]
    public void StatListConverter_writes_stat_list_back_as_object()
    {
        var sb = new StatBlock
        {
            Stats = new() { new() { Name = "str", Value = 12 }, new() { Name = "wis", Value = 8 } },
        };

        var json = JsonSerializer.Serialize(sb);
        using var doc = JsonDocument.Parse(json);
        var stats = doc.RootElement.GetProperty("stats");

        Assert.Equal(12, stats.GetProperty("str").GetInt32());
        Assert.Equal(8, stats.GetProperty("wis").GetInt32());
    }

    [Fact]
    public void Maps_named_properties_to_snake_case_json()
    {
        var sb = JsonSerializer.Deserialize<StatBlock>("""
        {
          "level": 5,
          "hp": 100,
          "current_hp": 42,
          "armour": 3,
          "modifier_formula": "floor((x-10)/2)"
        }
        """)!;

        Assert.Equal(5, sb.Level);
        Assert.Equal(100, sb.MaxHealth);
        Assert.Equal(42, sb.CurrentHealth);
        Assert.Equal(3, sb.MaxArmour);
        Assert.Equal("floor((x-10)/2)", sb.ModifierFormula);
    }

    [Fact]
    public void Round_trips_stats_and_scalar_fields()
    {
        var original = new StatBlock
        {
            Level = 3,
            MaxHealth = 50,
            CurrentHealth = 25,
            Stats = new() { new() { Name = "str", Value = 14 } },
            SpecialStats = new() { new() { Name = "luck", Value = 2 } },
        };

        var restored = JsonSerializer.Deserialize<StatBlock>(JsonSerializer.Serialize(original))!;

        Assert.Equal(3, restored.Level);
        Assert.Equal(50, restored.MaxHealth);
        Assert.Equal(25, restored.CurrentHealth);
        Assert.Equal(14, restored.Stats!.Single().Value);
        Assert.Equal(2, restored.SpecialStats!.Single().Value);
    }

    [Fact]
    public void Deserializes_legacy_buffs_field()
    {
        var sb = JsonSerializer.Deserialize<StatBlock>("""
        { "buffs": [ { "Name": "Old", "Type": "Cumulative", "Formulae": ["str+=1"], "Stacks": 2 } ] }
        """)!;

        var buff = Assert.Single(sb.LegacyBuffs!);
        Assert.Equal("Old", buff.Name);
        Assert.True(buff.Effects.Single().IsCumulative);
    }
}
