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

    private int _sequence;

    public event System.Action<ICommand> CommandApplied;

    public bool Apply(ICommand command)
    {
        if (command.RequiresActiveTurn && !_state.CanPlayerAct(command.IssuingPlayer))
        {
            GridLog.Warning($"[TurnExecutor] off-turn command from player {command.IssuingPlayer} " +
                            $"(active={_state.ActivePlayer})");
            return false;
        }
        if (!command.ApplyTo(_state)) return false;

        _sequence++;
        // divergence-detection seam (later): var hash = _state.ComputeHash(); StateHashed?.Invoke(_sequence, hash);
        CommandApplied?.Invoke(command);
        return true;
    }
}
