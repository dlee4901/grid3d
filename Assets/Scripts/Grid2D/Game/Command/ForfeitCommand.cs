public sealed class ForfeitCommand : Command
{
    public override int IssuingPlayer { get; }
    public override bool RequiresActiveTurn => false;

    public ForfeitCommand(int issuingPlayer) => IssuingPlayer = issuingPlayer;

    internal override bool ApplyTo(GridState state) => state.Forfeit(IssuingPlayer);
}
