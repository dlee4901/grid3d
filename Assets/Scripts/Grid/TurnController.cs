using UnityEngine;

public class TurnController : LoggableBehaviour
{
    private GridManager _gridManager;
    private bool _timersEnabled;
    private readonly PlayerTimers _timers = new();

    private GridState State => _gridManager.GridState;

    public bool TimersEnabled => _timersEnabled;
    public bool IsLocalTurn => !State.IsGameOver && State.ActivePlayer == _gridManager.LocalSeat;
    public int RemainingMs(int player) => _timers.RemainingMs(player);
    public int ElapsedMs(int seat) => _timers.ElapsedMs(seat);

    public void Init(GridManager gridManager, bool timersEnabled)
    {
        _gridManager = gridManager;
        _timersEnabled = timersEnabled;
        _timers.Init(State.Definition.PlayerCount);
        _timers.SeatExpired += OnTimerExpired;
        _gridManager.StateChanged += OnStateChanged;
        OnStateChanged();
    }

    private void OnStateChanged()
    {
        for (var p = 1; p <= State.Definition.PlayerCount; p++)
        {
            _timers.SetTimeMs(p, State.GetTimeMs(p));
            _timers.SetRunning(p, _timersEnabled && p == State.ActivePlayer && !State.IsGameOver);
        }
    }

    private void Update()
    {
        if (_gridManager == null || !_gridManager.IsGameStarted) return;
        _timers.Tick(Time.deltaTime);
    }

    private void OnTimerExpired(int player)
    {
        if (player == State.ActivePlayer && IsLocalTurn) _gridManager.Submit(seat => new ForfeitCommand(seat));
    }

    private void OnDestroy()
    {
        _timers.SeatExpired -= OnTimerExpired;
        if (_gridManager != null) _gridManager.StateChanged -= OnStateChanged;
    }
}
