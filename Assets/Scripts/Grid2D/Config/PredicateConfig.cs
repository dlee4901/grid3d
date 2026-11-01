#nullable enable
using System.Collections.Generic;

public enum ConditionOperation { Match, And, Or, Not }
public enum ConditionValueType { Int, Bool, String }

public sealed class PredicateConfig
{
    public ConditionOperation Operation { get; init; }

    public string? Field { get; init; }
    public string? Comparison { get; init; }

    public ConditionValueType ValueType { get; init; }

    public int IntValue { get; init; }
    public bool BoolValue { get; init; }
    public string? StringValue { get; init; }

    public List<PredicateConfig>? Children { get; init; }
}
