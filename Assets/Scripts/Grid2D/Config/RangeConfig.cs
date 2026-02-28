using System.Collections.Generic;

public class IntRange
{
    public int Start { get; set; }
    public int End { get; set; }
    
    public IntRange(int start, int end)
    {
        Start = start;
        End = end;
    }
    
    public int[] GetValues(bool includeLower=true, bool includeUpper=true)
    {
        var values = new List<int>();
        var start = includeLower ? Start : Start + 1;
        var end = includeUpper ? End : End - 1;
        for (var i = start; i <= end; i++)
            values.Add(i);
        return values.ToArray();
    }
}

public class IntRangePattern
{
    public int Start { get; set; } = 0;
    public int Span { get; set; }
    public int Gap { get; set; }
    
    public IntRangePattern(int start, int span, int gap)
    {
        Start = start;
        Span = span;
        Gap = gap;
    }
    
    public List<IntRange> GetIntRanges(int maxDistance)
    {
        var intRanges = new List<IntRange>();
        if (Span == 0 || Gap == 0) return null;
        for (var i = Start; i <= maxDistance; i += Span + Gap)
            intRanges.Add(new IntRange(i, i + Span - 1));
        return intRanges;
    }
}