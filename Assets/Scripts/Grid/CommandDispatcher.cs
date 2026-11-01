using System;

public sealed class CommandDispatcher
{
    private readonly TurnExecutor _executor;
    private readonly Func<int> _localSeat;

    public CommandDispatcher(TurnExecutor executor, Func<int> localSeat)
    {
        _executor = executor;
        _localSeat = localSeat;
    }

    public int LocalSeat => _localSeat();
    public bool Enabled { get; set; } = true;

    public bool Submit(Func<int, Command> build) => Enabled && _executor.Apply(build(_localSeat()));
}
