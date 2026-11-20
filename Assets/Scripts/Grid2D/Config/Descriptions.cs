using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class Descriptions
{
    private static readonly Regex TokenPattern = new(@"\{(\w+)\}");

    public static IEnumerable<string> Tokens(string description)
    {
        if (string.IsNullOrEmpty(description)) yield break;
        foreach (Match match in TokenPattern.Matches(description)) yield return match.Groups[1].Value;
    }

    public static string Replace(string description, System.Func<string, string> resolve)
        => string.IsNullOrEmpty(description) ? "" : TokenPattern.Replace(description, match => resolve(match.Groups[1].Value));

    public static bool TryGetValueRange(EffectConfig config, string token, GridState state, out int min, out int max)
    {
        min = max = 0;
        var effect = FindEffect(config, token, out var matches);
        return matches == 1 && effect.TryGetValueRange(state, out min, out max);
    }

    public static bool TryGetEffect(EffectConfig config, string token, out AbilityEffect effect)
    {
        effect = FindEffect(config, token, out var matches);
        return matches == 1;
    }

    public static IEnumerable<string> Validate(EffectConfig config)
    {
        foreach (var token in Tokens(config.Description))
        {
            FindEffect(config, token, out var matches);
            if (matches != 1) yield return $"'{config.Id}' description token {{{token}}} matches {matches} effects (needs exactly 1)";
        }
    }

    private static AbilityEffect FindEffect(EffectConfig config, string token, out int matches)
    {
        matches = 0;
        AbilityEffect found = null;
        if (config.Effects == null) return null;
        foreach (var effect in config.Effects)
        {
            if (!effect.MatchesToken(token)) continue;
            matches++;
            found = effect;
        }
        return found;
    }
}
