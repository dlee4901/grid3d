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

    private int _sequence;   // canonical command ordinal — identical on every peer

    public bool Apply(ICommand command)
    {
        // (1) turn gate — cross-cutting, identical verdict on every peer
        if (command.RequiresActiveTurn && !_state.CanPlayerAct(command.IssuingPlayer))
        {
            GridLog.Warning($"[TurnExecutor] off-turn command from player {command.IssuingPlayer} " +
                            $"(active={_state.ActivePlayer})");
            return false;
        }
        // (2) the single deterministic mutation
        if (!command.ApplyTo(_state)) return false;

        // (3) networking seams (stubs now — divergence detection attaches here later)
        _sequence++;
        // var hash = _state.ComputeHash();
        // CommandApplied?.Invoke(_sequence, hash);
        return true;
    }
}
