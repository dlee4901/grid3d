public sealed class EndTurnCommand : ICommand
{
    public int IssuingPlayer { get; }
    public int ElapsedMs { get; }               // wall-clock time the player used this turn (stamped by the issuing client)
    public bool RequiresActiveTurn => true;     // only the active player may end their turn

    public EndTurnCommand(int issuingPlayer, int elapsedMs = 0)
    {
        IssuingPlayer = issuingPlayer;
        ElapsedMs = elapsedMs;
    }

    public bool ApplyTo(GridState state)
    {
        state.SpendTime(state.ActivePlayer, ElapsedMs);   // deterministic: integer deduction from a command primitive
        state.AdvanceTurn();
        return true;
    }
}
