public sealed class SpawnTeamsCommand : ICommand
{
    public int IssuingPlayer => 0;
    public bool RequiresActiveTurn => false;

    public bool ApplyTo(GridState state)
    {
        state.SpawnTeams();
        return true;
    }
}