using System;
using System.Linq;

public static class QueryCompiler<T>
{
    public static Func<T, bool> Compile(QueryNode node)
        => node.Operation switch
        {
            QueryOperation.Match => CompileMatch(node),
            QueryOperation.And   => CompileAnd(node),
            QueryOperation.Or    => CompileOr(node),
            QueryOperation.Not   => CompileNot(node),
            _ => throw new InvalidOperationException()
        };

    private static Func<T, bool> CompileMatch(QueryNode node)
        => node.ValueType switch
        {
            QueryValueType.Int   => CompileComparison(node, node.IntValue),
            QueryValueType.Bool  => CompileComparison(node, node.BoolValue),
            QueryValueType.String => CompileComparison(node, node.StringValue),
            _ => throw new InvalidOperationException()
        };

    private static Func<T, bool> CompileComparison<TValue>(
        QueryNode node, TValue rhs)
        where TValue : IComparable<TValue>
    {
        var accessor = AccessorRegistry<T>.Get<TValue>(node.Field!);

        return node.Comparison switch
        {
            "==" => x => accessor(x).CompareTo(rhs) == 0,
            "!=" => x => accessor(x).CompareTo(rhs) != 0,
            ">"  => x => accessor(x).CompareTo(rhs) > 0,
            "<"  => x => accessor(x).CompareTo(rhs) < 0,
            ">=" => x => accessor(x).CompareTo(rhs) >= 0,
            "<=" => x => accessor(x).CompareTo(rhs) <= 0,
            _ => throw new InvalidOperationException("Invalid comparison")
        };
    }

    private static Func<T, bool> CompileAnd(QueryNode node)
    {
        var compiled = node.Children!.Select(Compile).ToArray();
        return x => compiled.All(p => p(x));
    }

    private static Func<T, bool> CompileOr(QueryNode node)
    {
        var compiled = node.Children!.Select(Compile).ToArray();
        return x => compiled.Any(p => p(x));
    }

    private static Func<T, bool> CompileNot(QueryNode node)
    {
        var inner = Compile(node.Children![0]);
        return x => !inner(x);
    }
}