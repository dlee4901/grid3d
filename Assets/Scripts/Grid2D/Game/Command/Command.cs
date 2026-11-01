public abstract class Command
{
    public abstract int IssuingPlayer { get; }
    public abstract bool RequiresActiveTurn { get; }
    internal abstract bool ApplyTo(GridState state);
}
