using UnityEngine;

public class TurnController : LoggableBehaviour
{
    private GridManager _gridManager;
    private bool _timersEnabled;
    private readonly PlayerTimers _timers = new();
    private int _timedPlayer;
    private bool _autoEndPending;

    private GridState State => _gridManager.GridState;

    public bool TimersEnabled => _timersEnabled;
    public bool CanEndTurn => !State.IsGameOver && State.ActivePlayer == _gridManager.LocalSeat;
    public int RemainingMs(int player) => _timers.RemainingMs(player);

    public void Init(GridManager gridManager, bool timersEnabled)
    {
        _gridManager = gridManager;
        _timersEnabled = timersEnabled;
        _timers.Init(State.Definition.PlayerCount);
        _timers.SeatExpired += OnTimerExpired;
        _gridManager.StateChanged += OnStateChanged;
        OnStateChanged();
    }

    public void EndTurn()
    {
        if (!CanEndTurn) return;
        _gridManager.Submit(seat => new EndTurnCommand(seat, _timers.ElapsedMs(seat)));
    }

    private void OnStateChanged()
    {
        SyncTimers();
        _autoEndPending = CanEndTurn && State.GetAvailableEntityPositions().Count == 0;
    }

    private void SyncTimers()
    {
        var turnChanged = State.ActivePlayer != _timedPlayer;
        _timedPlayer = State.ActivePlayer;
        for (var p = 1; p <= State.Definition.PlayerCount; p++)
        {
            if (turnChanged || !_timers.IsRunning(p)) _timers.SetTimeMs(p, State.GetTimeMs(p));
            _timers.SetRunning(p, _timersEnabled && p == State.ActivePlayer && !State.IsGameOver);
        }
    }

    private void Update()
    {
        if (_gridManager == null || !_gridManager.IsGameStarted) return;
        if (_autoEndPending)
        {
            _autoEndPending = false;
            EndTurn();
        }
        _timers.Tick(Time.deltaTime);
    }

    private void OnTimerExpired(int player)
    {
        if (player == State.ActivePlayer) EndTurn();
    }

    private void OnDestroy()
    {
        _timers.SeatExpired -= OnTimerExpired;
        if (_gridManager != null) _gridManager.StateChanged -= OnStateChanged;
    }
}
