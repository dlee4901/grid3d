using System;

[Flags]
public enum Keyword { None = 0, Finisher = 1, Double = 2, Opener = 4 }

public static class KeywordText
{
    public static readonly Keyword[] All = { Keyword.Opener, Keyword.Finisher, Keyword.Double };

    public static string Describe(Keyword keyword) => keyword switch
    {
        Keyword.Opener => "Using this does not stop this unit from using its other abilities this turn.",
        Keyword.Finisher => "Can still be used after this unit has used a different ability this turn.",
        Keyword.Double => "Can be used a second time in the same turn.",
        _ => ""
    };
}
