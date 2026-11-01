public sealed class SpawnTeamsCommand : Command
{
    public override int IssuingPlayer => 0;
    public override bool RequiresActiveTurn => false;

    internal override bool ApplyTo(GridState state)
    {
        state.SpawnTeams();
        state.ResolveForcedPasses();
        return true;
    }
}