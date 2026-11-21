public static class Var
{
    public static readonly Value Distance = new VarValue(ctx => ctx.Distance);
    public static readonly Value Count = new VarValue(ctx => ctx.Triggered?.Count ?? 0);
}
