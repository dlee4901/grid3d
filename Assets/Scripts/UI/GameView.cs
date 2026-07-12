using UnityEngine;
using UnityEngine.UI;

public class GameView : StateView
{
    // Local     — one machine drives every seat's clock and commits its timeouts (hotseat / testing).
    // Networked — every clock ticks as a prediction, but this client only ends/auto-times-out its own
    //             seat; remote seats snap to the bank when their EndTurnCommand arrives (P2P).
    // Off       — no ticking, no auto-timeout (manual end turn still allowed).
    public enum TimerMode { Local, Networked, Off }

    [SerializeField] private ManaCounter _manaCounter;
    [SerializeField] private InteractableUI _endTurn;
    [SerializeField] private VerticalLayoutGroup _container;
    [SerializeField] private PlayerTimer _playerTimerPrefab;
    [SerializeField] private TimerMode _timerMode = TimerMode.Local;
    [SerializeField] private int _localSeat = 1;   // the seat THIS client controls in Networked mode

    private PlayerTimer[] _timers;    // index 1..PlayerCount
    private int _shownActivePlayer;   // 0 = uninitialised

    protected override void Start()
    {
        InitPlayerTimers();                              // build _timers before base.Start() runs Refresh()
        base.Start();                                    // subscribes StateChanged + initial Refresh()
        if (_endTurn != null) _endTurn.OnClickCompleted += EndActiveTurn;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_endTurn != null) _endTurn.OnClickCompleted -= EndActiveTurn;
    }

    protected override void Refresh()
    {
        var state = _gridManager.State;
        var players = state.Definition.PlayerCount;

        _manaCounter.SetManaCount(state.GetMana(state.ActivePlayer));

        var turnChanged = state.ActivePlayer != _shownActivePlayer;
        _shownActivePlayer = state.ActivePlayer;

        for (var p = 1; p <= players; p++)
        {
            var isActive = _timerMode != TimerMode.Off && p == state.ActivePlayer;
            // Snap idle timers to the authoritative bank, and snap everyone on a turn change (that's
            // when a bank actually moved). Never stomp a mid-turn running clock's local prediction.
            if (turnChanged || !_timers[p].IsRunning)
                _timers[p].SetTimeMs(state.GetTimeMs(p));
            _timers[p].SetRunning(isActive);
        }

        // Only surface the end-turn button for a turn this client is allowed to end.
        if (_endTurn != null)
            _endTurn.gameObject.SetActive(CanEndTurnFor(state.ActivePlayer));
    }

    private void InitPlayerTimers()
    {
        var definition = _gridManager.State.Definition;
        _timers = new PlayerTimer[definition.PlayerCount + 1];
        for (var p = 1; p <= definition.PlayerCount; p++)
        {
            var timer = Instantiate(_playerTimerPrefab, _container.transform, true);
            timer.SetLabel("Player " + p);
            timer.SetTimeMs(_gridManager.State.GetTimeMs(p));
            var seat = p;                                // capture for the closure
            timer.Expired += () => OnTimerExpired(seat);
            _timers[p] = timer;
        }
    }

    // Manual end turn (end-turn button).
    private void EndActiveTurn()
    {
        var active = _gridManager.State.ActivePlayer;
        if (CanEndTurnFor(active)) SubmitEndTurn(active);
    }

    // Auto end turn on a local countdown reaching zero.
    private void OnTimerExpired(int player)
    {
        if (_timerMode == TimerMode.Off) return;
        if (!CanEndTurnFor(player)) return;                     // Networked: don't commit the opponent's timeout
        if (_gridManager.State.ActivePlayer != player) return;  // stale (turn already advanced)
        SubmitEndTurn(player);
    }

    private void SubmitEndTurn(int player)
    {
        var elapsed = _timers[player] != null ? _timers[player].ElapsedMs : 0;
        _gridManager.Submit(new EndTurnCommand(player, elapsed));
    }

    // Which seats this client is authoritative for.
    private bool CanEndTurnFor(int player)
        => _timerMode != TimerMode.Networked || player == _localSeat;
}
