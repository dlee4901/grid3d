public interface ICommand
{
    int IssuingPlayer { get; }
    bool RequiresActiveTurn { get; }   // turn-gated commands return true
    bool ApplyTo(GridState state);
}
