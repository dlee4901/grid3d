using System;

public abstract class Value
{
    internal abstract int Eval(in EffectContext ctx);

    public static implicit operator Value(int constant) => new ConstValue(constant);

    public static Value operator +(Value a, Value b) => new BinaryValue(a, b, (x, y) => x + y);
    public static Value operator -(Value a, Value b) => new BinaryValue(a, b, (x, y) => x - y);
    public static Value operator *(Value a, Value b) => new BinaryValue(a, b, (x, y) => x * y);

    public static Condition operator <(Value a, Value b) => new CompareCondition(a, b, (x, y) => x < y);
    public static Condition operator >(Value a, Value b) => new CompareCondition(a, b, (x, y) => x > y);
    public static Condition operator <=(Value a, Value b) => new CompareCondition(a, b, (x, y) => x <= y);
    public static Condition operator >=(Value a, Value b) => new CompareCondition(a, b, (x, y) => x >= y);

    public static Value If(Condition when, Value then, Value otherwise) => new IfValue(when, then, otherwise);
}

internal sealed class ConstValue : Value
{
    private readonly int _value;
    public ConstValue(int value) => _value = value;
    internal override int Eval(in EffectContext ctx) => _value;
}

internal sealed class BinaryValue : Value
{
    private readonly Value _a, _b;
    private readonly Func<int, int, int> _op;
    public BinaryValue(Value a, Value b, Func<int, int, int> op) { _a = a; _b = b; _op = op; }
    internal override int Eval(in EffectContext ctx) => _op(_a.Eval(ctx), _b.Eval(ctx));
}

internal sealed class IfValue : Value
{
    private readonly Condition _when;
    private readonly Value _then, _otherwise;
    public IfValue(Condition when, Value then, Value otherwise) { _when = when; _then = then; _otherwise = otherwise; }
    internal override int Eval(in EffectContext ctx) => _when.Eval(ctx) ? _then.Eval(ctx) : _otherwise.Eval(ctx);
}

internal sealed class VarValue : Value
{
    private readonly Func<EffectContext, int> _read;
    public VarValue(Func<EffectContext, int> read) => _read = read;
    internal override int Eval(in EffectContext ctx) => _read(ctx);
}
