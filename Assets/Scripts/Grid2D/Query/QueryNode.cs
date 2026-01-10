#nullable enable
using System.Collections.Generic;

public enum QueryOperation { Match, And, Or, Not }
public enum QueryValueType { Int, Bool, String}

public sealed class QueryNode
{
    public QueryOperation Operation { get; set; }

    public string? Field { get; set; }
    public string? Comparison { get; set; }

    public QueryValueType ValueType { get; set; }

    public int IntValue { get; set; }
    public bool BoolValue { get; set; }
    public string? StringValue { get; set; }

    public List<QueryNode>? Children { get; set; }
}
