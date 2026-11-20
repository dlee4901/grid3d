using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public sealed class TooltipContent
{
    public string Title;
    public int? Mana;
    public int? Cooldown;
    public string Body;
    public readonly Dictionary<string, TooltipContent> Links = new();
}

public static class TooltipText
{
    private static readonly string ValueColor = ColorUtility.ToHtmlStringRGB(new Color32(255, 216, 128, 255));
    private static readonly string BuffedColor = ColorUtility.ToHtmlStringRGB(new Color32(128, 232, 128, 255));
    private static readonly string NerfedColor = ColorUtility.ToHtmlStringRGB(new Color32(255, 128, 112, 255));
    private static readonly string LinkColor = ColorUtility.ToHtmlStringRGB(new Color32(176, 216, 255, 255));
    private static readonly string KindColor = ColorUtility.ToHtmlStringRGB(new Color32(160, 160, 168, 255));
    private const string Separator = "  ·  ";

    public static TooltipContent ForAbility(Ability ability, GridState state, Entity caster)
    {
        var config = ability.Config;
        var content = new TooltipContent
        {
            Title = Title(config),
            Mana = config.ManaCost,
            Cooldown = config.Cooldown > 1 ? config.Cooldown : null
        };
        var keywords = new List<string> { Kind("Ability") };
        foreach (var keyword in KeywordText.All)
        {
            if ((config.Keywords & keyword) == 0) continue;
            var id = $"kw:{keyword}";
            keywords.Add(Link(id, LinkColor, keyword.ToString()));
            content.Links[id] = new TooltipContent { Title = keyword.ToString(), Body = $"{Kind("Keyword")}\n{KeywordText.Describe(keyword)}" };
        }
        content.Body = string.Join(Separator, keywords) + "\n" + Body(config, state, caster, content);
        return content;
    }

    public static TooltipContent ForTrait(Trait trait, GridState state)
    {
        var content = new TooltipContent { Title = Title(trait.Config) };
        content.Body = $"{Kind("Trait")}\n{Body(trait.Config, state, null, content)}";
        return content;
    }

    public static TooltipContent ForStatus(Status status, GridState state)
    {
        var content = ForStatusConfig((StatusConfig)status.Config, state);
        content.Title = $"{content.Title} {status.Count}";
        return content;
    }

    private static TooltipContent ForStatusConfig(StatusConfig config, GridState state)
    {
        var content = new TooltipContent { Title = Title(config) };
        content.Body = $"{Kind("Status")}\n{Body(config, state, null, content)}";
        return content;
    }

    public static TooltipContent ForTerrain(TerrainType terrain, GridState state, int turns)
    {
        IdRegistry<TerrainConfig>.TryGet(terrain.ToString(), out var config);
        var content = new TooltipContent { Title = Title(config) };
        var kind = turns > 0 ? $"{Kind("Terrain")}{Separator}{Kind("Fades in")} {Value(ValueColor, turns.ToString())} {Kind("turns")}" : Kind("Terrain");
        content.Body = $"{kind}\n{Body(config, state, null, content)}";
        return content;
    }

    private static string Title(EffectConfig config) => config.DisplayName ?? config.Id;

    private static string Body(EffectConfig config, GridState state, Entity caster, TooltipContent content)
        => Descriptions.Replace(config.Description, token =>
        {
            if (!Descriptions.TryGetEffect(config, token, out var effect)) return $"{{{token}}}";
            if (effect is TerrainEffect terrainEffect && IdRegistry<TerrainConfig>.TryGet(terrainEffect.Terrain.ToString(), out var terrainConfig))
            {
                var id = $"tr:{terrainConfig.Id}";
                terrainEffect.TryGetValueRange(state, out var turns, out _);
                content.Links[id] = ForTerrain(terrainEffect.Terrain, state, turns);
                return Link(id, LinkColor, terrainConfig.DisplayName ?? terrainConfig.Id);
            }
            if (caster != null && effect is DamageEffect damage && damage.Amount.TryGetConstant(out var baseAmount))
            {
                var roll = DamageBreakdown.ForCaster(state, caster, baseAmount);
                var id = $"val:{token}";
                content.Links[id] = Breakdown(token, roll);
                var color = roll.Amount > baseAmount ? BuffedColor : roll.Amount < baseAmount ? NerfedColor : ValueColor;
                return Link(id, color, roll.Amount.ToString());
            }
            if (!effect.TryGetValueRange(state, out var min, out var max)) return $"{{{token}}}";
            var value = min == max ? min.ToString() : $"{min}–{max}";
            if (effect is StatusEffect statusEffect && IdRegistry<StatusConfig>.TryGet(statusEffect.Status, out var statusConfig))
            {
                var id = $"st:{statusConfig.Id}";
                content.Links[id] = ForStatusConfig(statusConfig, state);
                return Link(id, LinkColor, $"{value} {statusConfig.DisplayName ?? statusConfig.Id}");
            }
            return Value(ValueColor, value);
        });

    private static TooltipContent Breakdown(string token, DamageRoll roll)
    {
        var lines = new List<string> { $"{Value(ValueColor, roll.BaseAmount.ToString())}  base" };
        foreach (var (source, delta) in roll.Modifiers)
            lines.Add($"{Value(delta >= 0 ? BuffedColor : NerfedColor, delta >= 0 ? $"+{delta}" : delta.ToString())}  {SourceName(source)}");
        return new TooltipContent { Title = $"{Humanize(token)} {roll.Amount}", Body = string.Join("\n", lines) };
    }

    private static string SourceName(string id)
    {
        if (id == null) return "Other";
        if (IdRegistry<StatusConfig>.TryGet(id, out var status)) return status.DisplayName ?? status.Id;
        if (IdRegistry<TraitConfig>.TryGet(id, out var trait)) return trait.DisplayName ?? trait.Id;
        return id;
    }

    private static string Humanize(string token) => Regex.Replace(token, "(?<=[a-z])(?=[A-Z])", " ");

    private static string Kind(string text) => $"<color=#{KindColor}>{text}</color>";

    private static string Value(string color, string text) => $"<color=#{color}>{text}</color>";

    private static string Link(string id, string color, string text) => $"<link=\"{id}\"><color=#{color}>{text}</color></link>";
}
