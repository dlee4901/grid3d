#nullable enable
using System.Collections.Generic;

public enum ConditionOperation { Match, And, Or, Not }
public enum ConditionValueType { Int, Bool, String }

public sealed class PredicateConfig
{
    public ConditionOperation Operation { get; set; }

    public string? Field { get; set; }
    public string? Comparison { get; set; }

    public ConditionValueType ValueType { get; set; }

    public int IntValue { get; set; }
    public bool BoolValue { get; set; }
    public string? StringValue { get; set; }

    public List<PredicateConfig>? Children { get; set; }
}
