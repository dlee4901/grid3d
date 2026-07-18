public sealed class EndTurnCommand : ICommand
{
    public int IssuingPlayer { get; }
    public int ElapsedMs { get; }
    public bool RequiresActiveTurn => true;

    public EndTurnCommand(int issuingPlayer, int elapsedMs = 0)
    {
        IssuingPlayer = issuingPlayer;
        ElapsedMs = elapsedMs;
    }

    public bool ApplyTo(GridState state)
    {
        state.SpendTime(state.ActivePlayer, ElapsedMs);
        state.AdvanceTurn();
        return true;
    }
}
