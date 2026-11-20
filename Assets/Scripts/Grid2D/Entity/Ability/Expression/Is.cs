public static class Is
{
    public static readonly Condition AbilityDamage = new FuncCondition(ctx => ctx.Damage != null && ctx.Damage.FromAbility);
    public static Condition DamageFrom(string id) => new FuncCondition(ctx => ctx.Damage?.Cause == id);
    public static Condition OnTerrain(TerrainType terrain)
        => new FuncCondition(ctx => ctx.State.TryGetTerrain(ctx.Position, out var current) && current == terrain);
}
