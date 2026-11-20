public sealed class TerrainEffect : AbilityEffect
{
    public TerrainType Terrain { get; init; }
    public int Rounds { get; init; } = 2;

    protected override string DefaultDescriptionToken => Terrain.ToString();

    public override bool TryGetValueRange(GridState state, out int min, out int max)
    {
        min = max = Turns(state);
        return true;
    }

    internal override void Apply(in EffectContext ctx) => ctx.State.SetTerrain(ctx.Position, Terrain, Turns(ctx.State));

    private int Turns(GridState state) => Terrain == TerrainType.Plain ? 0 : Rounds * state.Definition.PlayerCount;
}
