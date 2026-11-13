public static class Is
{
    public static readonly Condition AbilityDamage = new FuncCondition(ctx => ctx.Damage != null && ctx.Damage.FromAbility);
    public static Condition DamageFrom(string id) => new FuncCondition(ctx => ctx.Damage?.Cause == id);
}
