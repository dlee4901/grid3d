using System;

public abstract class Condition
{
    public static readonly Condition Always = new ConstCondition(true);
    internal abstract bool Eval(in EffectContext ctx);
}

internal sealed class ConstCondition : Condition
{
    private readonly bool _value;
    public ConstCondition(bool value) => _value = value;
    internal override bool Eval(in EffectContext ctx) => _value;
}

internal sealed class CompareCondition : Condition
{
    private readonly Value _a, _b;
    private readonly Func<int, int, bool> _op;
    public CompareCondition(Value a, Value b, Func<int, int, bool> op) { _a = a; _b = b; _op = op; }
    internal override bool Eval(in EffectContext ctx) => _op(_a.Eval(ctx), _b.Eval(ctx));
}
