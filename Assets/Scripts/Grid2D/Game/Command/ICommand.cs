public interface ICommand
{
    int IssuingPlayer { get; }
    bool RequiresActiveTurn { get; }
    bool ApplyTo(GridState state);
}
