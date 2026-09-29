using System.Text.Json;
using Xunit;

namespace OW3N.Tests.Unit;

/// <summary>
/// Tests for <see cref="BuffConverter"/>, which must deserialize both the current "Effects"
/// format and the legacy "Formulae"/"Cumulative" format, and round-trip the current format.
/// </summary>
public class BuffSerializationTests
{
    private static Buff Deserialize(string json) =>
        JsonSerializer.Deserialize<Buff>(json)!;

    [Fact]
    public void Reads_new_effects_format()
    {
        var buff = Deserialize("""
        {
          "Name": "Shield Wall",
          "Icon": "shield",
          "Type": "Equipment",
          "Effects": [
            { "Formula": "armour+=2", "IsCumulative": false, "Stacks": 1 }
          ]
        }
        """);

        Assert.Equal("Shield Wall", buff.Name);
        Assert.Equal("shield", buff.Icon);
        Assert.Equal(BuffType.Equipment, buff.Type);
        var effect = Assert.Single(buff.Effects);
        Assert.Equal("armour+=2", effect.Formula);
        Assert.False(effect.IsCumulative);
    }

    [Fact]
    public void Reads_legacy_cumulative_formulae_format()
    {
        var buff = Deserialize("""
        {
          "Name": "Poison",
          "Type": "Cumulative",
          "Formulae": [ "health.value-=1" ],
          "Stacks": 3
        }
        """);

        // Legacy "Cumulative" type is migrated to Status.
        Assert.Equal(BuffType.Status, buff.Type);
        var effect = Assert.Single(buff.Effects);
        Assert.Equal("health.value-=1", effect.Formula);
        Assert.True(effect.IsCumulative);
        Assert.Equal(3, effect.Stacks);
    }

    [Fact]
    public void Reads_legacy_non_cumulative_formulae_as_single_stack_effects()
    {
        var buff = Deserialize("""
        {
          "Name": "Ring of Might",
          "Type": "Equipment",
          "Formulae": [ "str+=1", "con+=1" ]
        }
        """);

        Assert.Equal(BuffType.Equipment, buff.Type);
        Assert.Equal(2, buff.Effects.Count);
        Assert.All(buff.Effects, e =>
        {
            Assert.False(e.IsCumulative);
            Assert.Equal(1, e.Stacks);
        });
    }

    [Fact]
    public void Defaults_type_to_status_when_missing()
    {
        var buff = Deserialize("""{ "Name": "Mystery", "Effects": [] }""");
        Assert.Equal(BuffType.Status, buff.Type);
        Assert.Equal("shield", buff.Icon); // default icon
    }

    [Fact]
    public void Round_trips_current_format()
    {
        var original = new Buff
        {
            Name = "Regen",
            Icon = "heart",
            Type = BuffType.Skill,
            Effects = new()
            {
                new() { Formula = "health.value+=2", IsCumulative = true, Stacks = 2 },
            },
        };

        var json = JsonSerializer.Serialize(original);
        var restored = Deserialize(json);

        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.Icon, restored.Icon);
        Assert.Equal(original.Type, restored.Type);
        var effect = Assert.Single(restored.Effects);
        Assert.Equal("health.value+=2", effect.Formula);
        Assert.True(effect.IsCumulative);
        Assert.Equal(2, effect.Stacks);
    }

    [Fact]
    public void HasCumulativeEffects_reflects_effect_flags()
    {
        Assert.True(new Buff { Effects = { new() { IsCumulative = true } } }.HasCumulativeEffects);
        Assert.False(new Buff { Effects = { new() { IsCumulative = false } } }.HasCumulativeEffects);
    }
}
