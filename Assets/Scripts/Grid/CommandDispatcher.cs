public sealed class CommandDispatcher
{
    private readonly TurnExecutor _executor;

    public CommandDispatcher(TurnExecutor executor) => _executor = executor;

    public bool Submit(ICommand command) => _executor.Apply(command);
}
