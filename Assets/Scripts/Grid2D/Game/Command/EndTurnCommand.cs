public sealed class EndTurnCommand : Command
{
    public override int IssuingPlayer { get; }
    public int ElapsedMs { get; }
    public override bool RequiresActiveTurn => true;

    public EndTurnCommand(int issuingPlayer, int elapsedMs = 0)
    {
        IssuingPlayer = issuingPlayer;
        ElapsedMs = elapsedMs;
    }

    internal override bool ApplyTo(GridState state)
    {
        state.SpendTime(state.ActivePlayer, ElapsedMs);
        state.AdvanceTurn();
        return true;
    }
}
