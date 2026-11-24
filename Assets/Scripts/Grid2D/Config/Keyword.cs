using System;

[Flags]
public enum Keyword { None = 0, Opener = 1 }

public static class KeywordText
{
    public static readonly Keyword[] All = { Keyword.Opener };

    public static string Describe(Keyword keyword) => keyword switch
    {
        Keyword.Opener => "If this is the first ability used this turn, this unit can act again.",
        _ => ""
    };
}
