using Xunit;

namespace OW3N.Tests.Unit;

/// <summary>
/// Tests for the JSON-backed convenience properties on <see cref="BuffTemplate"/> and
/// <see cref="CharacterPreset"/>, plus <see cref="BuffTemplate.IsOutOfSync"/>.
/// </summary>
public class ModelSerializationTests
{
    [Fact]
    public void BuffTemplate_effects_round_trip_through_json_column()
    {
        var template = new BuffTemplate
        {
            Effects = new()
            {
                new() { Formula = "str+=1", IsCumulative = false, Stacks = 1 },
                new() { Formula = "health.value-=1", IsCumulative = true, Stacks = 5 },
            },
        };

        // Setting Effects serialises into EffectsJson...
        Assert.Contains("Formula", template.EffectsJson);

        // ...and getting Effects deserialises from it.
        var reloaded = new BuffTemplate { EffectsJson = template.EffectsJson };
        Assert.Equal(2, reloaded.Effects.Count);
        Assert.Equal("health.value-=1", reloaded.Effects[1].Formula);
        Assert.True(reloaded.Effects[1].IsCumulative);
    }

    [Fact]
    public void BuffTemplate_defaults_to_empty_effect_list()
    {
        Assert.Empty(new BuffTemplate().Effects);
    }

    [Fact]
    public void BuffTemplate_IsOutOfSync_detects_differences()
    {
        var source = new BuffTemplate { Name = "A", Icon = "x", Type = BuffType.Status, EffectsJson = "[]" };

        Assert.False(source.IsOutOfSync(null));
        Assert.False(new BuffTemplate { Name = "A", Icon = "x", Type = BuffType.Status, EffectsJson = "[]" }.IsOutOfSync(source));
        Assert.True(new BuffTemplate { Name = "B", Icon = "x", Type = BuffType.Status, EffectsJson = "[]" }.IsOutOfSync(source));
        Assert.True(new BuffTemplate { Name = "A", Icon = "y", Type = BuffType.Status, EffectsJson = "[]" }.IsOutOfSync(source));
        Assert.True(new BuffTemplate { Name = "A", Icon = "x", Type = BuffType.Skill, EffectsJson = "[]" }.IsOutOfSync(source));
        Assert.True(new BuffTemplate { Name = "A", Icon = "x", Type = BuffType.Status, EffectsJson = "[{}]" }.IsOutOfSync(source));
    }

    [Fact]
    public void CharacterPreset_statblock_round_trips_through_json_column()
    {
        var preset = new CharacterPreset
        {
            StatBlock = new StatBlock { Level = 2, Stats = new() { new() { Name = "str", Value = 11 } } },
        };

        Assert.NotNull(preset.StatBlockJson);

        var reloaded = new CharacterPreset { StatBlockJson = preset.StatBlockJson };
        Assert.Equal(2, reloaded.StatBlock!.Level);
        Assert.Equal(11, reloaded.StatBlock!.Stats!.Single().Value);
    }

    [Fact]
    public void CharacterPreset_gauges_round_trip_and_default_to_empty()
    {
        var preset = new CharacterPreset
        {
            Gauges = new() { new() { Name = "health", Max = 30, Colour = "red", GaugeType = GaugeType.IconBar } },
        };

        Assert.NotNull(preset.GaugesJson);

        var reloaded = new CharacterPreset { GaugesJson = preset.GaugesJson };
        var gauge = Assert.Single(reloaded.Gauges);
        Assert.Equal("health", gauge.Name);
        Assert.Equal(30, gauge.Max);
        Assert.Equal(GaugeType.IconBar, gauge.GaugeType);

        Assert.Empty(new CharacterPreset().Gauges);
    }
}
