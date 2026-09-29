using System.Linq;
using Xunit;

namespace OW3N.Tests.Unit;

/// <summary>
/// Tests for the <see cref="PlayerCharacter"/> POCO logic: the JSON-backed StatBlock property,
/// the saved-rolls string parser, and IValidatableObject validation.
/// </summary>
public class PlayerCharacterModelTests
{
    [Fact]
    public void StatBlock_setter_serialises_and_getter_deserialises()
    {
        var character = new PlayerCharacter
        {
            StatBlock = new StatBlock { Level = 4, Stats = new() { new() { Name = "str", Value = 16 } } },
        };

        Assert.NotNull(character.StatBlockJson);

        // A character rehydrated from persisted JSON exposes the same StatBlock.
        var rehydrated = new PlayerCharacter { StatBlockJson = character.StatBlockJson };
        Assert.Equal(4, rehydrated.StatBlock!.Level);
        Assert.Equal(16, rehydrated.StatBlock!.Stats!.Single().Value);
    }

    [Fact]
    public void StatBlock_set_to_null_clears_json()
    {
        var character = new PlayerCharacter { StatBlock = new StatBlock { Level = 1 } };
        character.StatBlock = null;
        Assert.Null(character.StatBlockJson);
    }

    [Fact]
    public void SavedRollsDict_parses_newline_and_colon_separated_entries()
    {
        var character = new PlayerCharacter { SavedRolls = "atk: 1d20+5\ndef: 1d20\n" };

        var dict = character.SavedRollsDict!;
        Assert.Equal("1d20+5", dict["atk"]);
        Assert.Equal("1d20", dict["def"]);
    }

    [Fact]
    public void SavedRollsDict_keeps_colons_in_the_value()
    {
        // Split with a limit of 2 means only the first colon separates key from value.
        var character = new PlayerCharacter { SavedRolls = "url: http://host:3000/roll" };
        Assert.Equal("http://host:3000/roll", character.SavedRollsDict!["url"]);
    }

    [Fact]
    public void SavedRollsDict_is_null_when_no_saved_rolls()
    {
        Assert.Null(new PlayerCharacter().SavedRollsDict);
    }

    [Fact]
    public void Validate_flags_gauge_without_name()
    {
        var character = new PlayerCharacter
        {
            Gauges = new List<Gauge> { new() { Name = "  ", Max = 10 } },
            StatBlock = new StatBlock { Stats = new() },
        };

        var results = character.Validate(new(character)).ToList();
        Assert.Contains(results, r => r.ErrorMessage == "Gauge name missing");
    }

    [Fact]
    public void Validate_flags_stat_without_name()
    {
        var character = new PlayerCharacter
        {
            Gauges = new List<Gauge>(),
            StatBlock = new StatBlock { Stats = new() { new() { Name = "", Value = 1 } } },
        };

        var results = character.Validate(new(character)).ToList();
        Assert.Contains(results, r => r.ErrorMessage == "Stat name missing");
    }

    [Fact]
    public void Validate_passes_for_well_formed_character()
    {
        var character = new PlayerCharacter
        {
            Gauges = new List<Gauge> { new() { Name = "health", Max = 10 } },
            StatBlock = new StatBlock { Stats = new() { new() { Name = "str", Value = 10 } } },
        };

        Assert.Empty(character.Validate(new(character)));
    }
}
