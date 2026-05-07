public sealed class TurnExecutor
{
    private readonly GridState _state;
    public IReadOnlyGridState State => _state;

    private TurnExecutor(GridState state)
    {
        _state = state;
    }

    public static TurnExecutor ForDefinition(GridDefinition definition)
        => new TurnExecutor(new GridState(definition));

    public bool Apply(ICommand command) => command.ApplyTo(_state);
}
