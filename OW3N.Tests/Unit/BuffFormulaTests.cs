using Xunit;

namespace OW3N.Tests.Unit;

/// <summary>
/// Unit tests for <see cref="BuffFormula"/>, the pure formula engine that parses buff effect
/// strings ("str+=1"), evaluates arithmetic with a variable context, and applies stacked effects.
/// </summary>
public class BuffFormulaTests
{
    // ---- Parse ----

    [Theory]
    [InlineData("str+=1", "str", "+=", "1")]
    [InlineData("str-=2", "str", "-=", "2")]
    [InlineData("health*=dex/2", "health", "*=", "dex/2")]
    [InlineData("mana/=2", "mana", "/=", "2")]
    [InlineData("hp=100", "hp", "=", "100")]
    public void Parse_extracts_target_operator_and_expression(string formula, string target, string op, string expr)
    {
        var parsed = BuffFormula.Parse(formula);
        Assert.NotNull(parsed);
        Assert.Equal((target, op, expr), parsed!.Value);
    }

    [Fact]
    public void Parse_lowercases_and_trims_target_and_trims_expression()
    {
        var parsed = BuffFormula.Parse("  STR  += 1 ");
        Assert.NotNull(parsed);
        Assert.Equal("str", parsed!.Value.target);
        Assert.Equal("1", parsed.Value.expr);
    }

    [Fact]
    public void Parse_prefers_compound_operator_over_bare_equals()
    {
        // "+=" must be detected before "=" so "str+=1" is not read as target "str+" set to "1".
        var parsed = BuffFormula.Parse("str+=1");
        Assert.Equal("+=", parsed!.Value.op);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nooperatorhere")]
    [InlineData("=5")]      // operator at index 0 -> empty target -> rejected
    [InlineData("str+=")]   // empty expression -> rejected
    public void Parse_returns_null_for_invalid_formulas(string formula)
    {
        Assert.Null(BuffFormula.Parse(formula));
    }

    [Fact]
    public void Parse_returns_null_for_null_input()
    {
        Assert.Null(BuffFormula.Parse(null!));
    }

    // ---- BuildContext ----

    [Fact]
    public void BuildContext_includes_stats_special_stats_and_gauges()
    {
        var sb = new StatBlock
        {
            Stats = new() { new() { Name = "str", Value = 10 }, new() { Name = "dex", Value = 14 } },
            SpecialStats = new() { new() { Name = "luck", Value = 3 } },
        };
        var gauges = new List<Gauge> { new() { Name = "health", Max = 50, Value = 20 } };

        var ctx = BuffFormula.BuildContext(sb, gauges);

        Assert.Equal(10, ctx["str"]);
        Assert.Equal(14, ctx["dex"]);
        Assert.Equal(3, ctx["luck"]);
        // Gauge contributes both its name and name.max, both mapped to Max (not the current Value).
        Assert.Equal(50, ctx["health"]);
        Assert.Equal(50, ctx["health.max"]);
    }

    [Fact]
    public void BuildContext_is_case_insensitive()
    {
        var sb = new StatBlock { Stats = new() { new() { Name = "Str", Value = 7 } } };
        var ctx = BuffFormula.BuildContext(sb, null);
        Assert.Equal(7, ctx["STR"]);
        Assert.Equal(7, ctx["str"]);
    }

    [Fact]
    public void BuildContext_treats_null_stat_value_as_zero_and_skips_empty_names()
    {
        var sb = new StatBlock
        {
            Stats = new() { new() { Name = "str", Value = null }, new() { Name = "", Value = 5 } },
        };
        var ctx = BuffFormula.BuildContext(sb, null);
        Assert.Equal(0, ctx["str"]);
        Assert.False(ctx.ContainsKey(""));
    }

    [Fact]
    public void BuildContext_handles_null_inputs()
    {
        var ctx = BuffFormula.BuildContext(null, null);
        Assert.Empty(ctx);
    }

    // ---- Evaluate ----

    [Fact]
    public void Evaluate_substitutes_variables_and_computes()
    {
        var ctx = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { ["dex"] = 10 };
        Assert.Equal(6, BuffFormula.Evaluate("dex/2+1", ctx));
    }

    [Fact]
    public void Evaluate_prefers_longest_variable_names_to_avoid_partial_matches()
    {
        var ctx = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["dex"] = 2,
            ["dexterity"] = 100,
        };
        // "dexterity" must not be corrupted by substituting "dex" inside it.
        Assert.Equal(100, BuffFormula.Evaluate("dexterity", ctx));
    }

    [Fact]
    public void Evaluate_returns_null_for_empty_or_invalid_expression()
    {
        var ctx = new Dictionary<string, double>();
        Assert.Null(BuffFormula.Evaluate("", ctx));
        Assert.Null(BuffFormula.Evaluate("   ", ctx));
        Assert.Null(BuffFormula.Evaluate("this is not math", ctx));
    }

    [Fact]
    public void Evaluate_uses_floating_point_division()
    {
        // DataTable.Compute performs floating-point division, so "5/2" yields 2.5.
        var ctx = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { ["x"] = 5 };
        Assert.Equal(2.5, BuffFormula.Evaluate("x/2", ctx));
    }

    // ---- ComputeEffective ----

    private static Buff Buff(params Effect[] effects) => new() { Effects = effects.ToList() };

    private static Effect Effect(string formula, bool cumulative = false, int stacks = 1)
        => new() { Formula = formula, IsCumulative = cumulative, Stacks = stacks };

    [Fact]
    public void ComputeEffective_applies_additive_effect()
    {
        var buffs = new[] { Buff(Effect("str+=2")) };
        Assert.Equal(12, BuffFormula.ComputeEffective(buffs, "str", 10, new()));
    }

    [Fact]
    public void ComputeEffective_applies_each_operator()
    {
        var ctx = new Dictionary<string, double>();
        Assert.Equal(15, BuffFormula.ComputeEffective(new[] { Buff(Effect("v+=5")) }, "v", 10, ctx));
        Assert.Equal(6, BuffFormula.ComputeEffective(new[] { Buff(Effect("v-=4")) }, "v", 10, ctx));
        Assert.Equal(30, BuffFormula.ComputeEffective(new[] { Buff(Effect("v*=3")) }, "v", 10, ctx));
        Assert.Equal(5, BuffFormula.ComputeEffective(new[] { Buff(Effect("v/=2")) }, "v", 10, ctx));
        Assert.Equal(99, BuffFormula.ComputeEffective(new[] { Buff(Effect("v=99")) }, "v", 10, ctx));
    }

    [Fact]
    public void ComputeEffective_ignores_division_by_zero()
    {
        var buffs = new[] { Buff(Effect("v/=0")) };
        Assert.Equal(10, BuffFormula.ComputeEffective(buffs, "v", 10, new()));
    }

    [Fact]
    public void ComputeEffective_only_applies_effects_targeting_the_requested_name()
    {
        var buffs = new[] { Buff(Effect("dex+=5"), Effect("str+=1")) };
        Assert.Equal(11, BuffFormula.ComputeEffective(buffs, "str", 10, new()));
    }

    [Fact]
    public void ComputeEffective_matches_target_case_insensitively()
    {
        var buffs = new[] { Buff(Effect("STR+=1")) };
        Assert.Equal(11, BuffFormula.ComputeEffective(buffs, "str", 10, new()));
    }

    [Fact]
    public void ComputeEffective_applies_cumulative_effect_once_per_stack()
    {
        var buffs = new[] { Buff(Effect("hp+=5", cumulative: true, stacks: 3)) };
        Assert.Equal(115, BuffFormula.ComputeEffective(buffs, "hp", 100, new()));
    }

    [Fact]
    public void ComputeEffective_applies_non_cumulative_effect_once_regardless_of_stacks()
    {
        var buffs = new[] { Buff(Effect("hp+=5", cumulative: false, stacks: 3)) };
        Assert.Equal(105, BuffFormula.ComputeEffective(buffs, "hp", 100, new()));
    }

    [Fact]
    public void ComputeEffective_treats_negative_stacks_as_zero_applications()
    {
        var buffs = new[] { Buff(Effect("hp+=5", cumulative: true, stacks: -2)) };
        Assert.Equal(100, BuffFormula.ComputeEffective(buffs, "hp", 100, new()));
    }

    [Fact]
    public void ComputeEffective_resolves_variables_in_expression()
    {
        var ctx = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase) { ["dex"] = 10 };
        var buffs = new[] { Buff(Effect("str+=dex/2")) };
        Assert.Equal(15, BuffFormula.ComputeEffective(buffs, "str", 10, ctx));
    }

    [Fact]
    public void ComputeEffective_skips_unparseable_and_unevaluatable_effects()
    {
        var buffs = new[] { Buff(Effect("garbage"), Effect("str+=notmath")) };
        Assert.Equal(10, BuffFormula.ComputeEffective(buffs, "str", 10, new()));
    }

    // ---- ComputeGaugeValueDelta ----

    [Fact]
    public void ComputeGaugeValueDelta_sums_cumulative_value_effects()
    {
        var buff = Buff(Effect("health.value-=5", cumulative: true), Effect("health.value+=2", cumulative: true));
        Assert.Equal(-3, BuffFormula.ComputeGaugeValueDelta(buff, "health", new()));
    }

    [Fact]
    public void ComputeGaugeValueDelta_ignores_non_cumulative_and_other_targets()
    {
        var buff = Buff(
            Effect("health.value-=5", cumulative: false),   // ignored: not cumulative
            Effect("mana.value-=5", cumulative: true),       // ignored: different gauge
            Effect("health+=5", cumulative: true));          // ignored: not the ".value" target
        Assert.Equal(0, BuffFormula.ComputeGaugeValueDelta(buff, "health", new()));
    }

    // ---- FormatDisplay / FormatTarget ----

    [Theory]
    [InlineData("str+=1", "Str + 1")]
    [InlineData("con*=dex/2", "Con × dex/2")]
    [InlineData("mana/=2", "Mana ÷ 2")]
    [InlineData("hp=100", "Hp = 100")]
    public void FormatDisplay_produces_human_readable_label(string formula, string expected)
    {
        Assert.Equal(expected, BuffFormula.FormatDisplay(formula));
    }

    [Fact]
    public void FormatDisplay_strips_value_suffix_from_gauge_targets()
    {
        Assert.Equal("Health - 2", BuffFormula.FormatDisplay("health.value-=2"));
    }

    [Fact]
    public void FormatDisplay_returns_original_string_when_unparseable()
    {
        Assert.Equal("nonsense", BuffFormula.FormatDisplay("nonsense"));
    }

    [Fact]
    public void FormatTarget_capitalises_and_strips_value_suffix()
    {
        Assert.Equal("Strength", BuffFormula.FormatTarget("strength"));
        Assert.Equal("Health", BuffFormula.FormatTarget("health.value"));
        Assert.Equal("", BuffFormula.FormatTarget(""));
    }
}
