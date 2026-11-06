using System.Collections.Generic;

public sealed class TurnExecutor
{
    private readonly GridState _state;
    private readonly List<Command> _log = new();

    public GridState State => _state;
    public IReadOnlyList<Command> Log => _log;
    public ulong StateHash { get; private set; }

    private TurnExecutor(GridState state)
    {
        _state = state;
        StateHash = state.ComputeHash();
    }

    public static TurnExecutor ForDefinition(GridDefinition definition)
        => new TurnExecutor(new GridState(definition));

    public event System.Action<Command> CommandApplied;

    public GridState Preview(Command command)
    {
        var state = new GridState(_state.Definition);
        foreach (var logged in _log) logged.ApplyTo(state);
        if (command.RequiresActiveTurn && !state.CanPlayerAct(command.IssuingPlayer)) return null;
        return command.ApplyTo(state) ? state : null;
    }

    public bool Apply(Command command)
    {
        var current = _state.ComputeHash();
        if (current != StateHash)
        {
            GridLog.Error($"[TurnExecutor] state changed outside a command after #{_log.Count}");
            StateHash = current;
        }
        if (command.RequiresActiveTurn && !_state.CanPlayerAct(command.IssuingPlayer))
        {
            GridLog.Warning($"[TurnExecutor] off-turn command from player {command.IssuingPlayer} " +
                            $"(active={_state.ActivePlayer})");
            return false;
        }
        if (!command.ApplyTo(_state)) return false;

        _log.Add(command);
        StateHash = _state.ComputeHash();
        CommandApplied?.Invoke(command);
        return true;
    }
}
