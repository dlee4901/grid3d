// The ONE place a locally-produced command enters the system. Input never touches
// TurnExecutor directly. Today: pass through to the executor. Later (networking):
//   - stamp IssuingPlayer from the authenticated LOCAL seat (not the payload)
//   - relay to peer, buffer + order, apply on confirm, then hash-compare
// Swapping in networking changes ONLY this class.
public sealed class CommandDispatcher
{
    private readonly TurnExecutor _executor;

    public CommandDispatcher(TurnExecutor executor) => _executor = executor;

    public bool Submit(ICommand command) => _executor.Apply(command);   // networking intercepts here
}
