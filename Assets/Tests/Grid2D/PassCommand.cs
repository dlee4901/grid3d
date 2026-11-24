public sealed class PassCommand : Command
{
    public override int IssuingPlayer { get; }
    public override bool RequiresActiveTurn => true;
    public int ElapsedMs { get; }

    public PassCommand(int issuingPlayer, int elapsedMs = 0)
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
