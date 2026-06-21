public sealed class EndTurnCommand : ICommand
{
    public int IssuingPlayer { get; }
    public bool RequiresActiveTurn => true;     // only the active player may end their turn

    public EndTurnCommand(int issuingPlayer) => IssuingPlayer = issuingPlayer;

    public bool ApplyTo(GridState state)
    {
        state.AdvanceTurn();
        return true;
    }
}
