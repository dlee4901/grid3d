public sealed class MoveEffect : AbilityEffect
{
    public MoveEffect() => Relation = Occupant.Empty;

    internal override void Apply(in EffectContext ctx)
        => ctx.State.ChangeEntityPosition(ctx.Source.Position, ctx.Position);
}
