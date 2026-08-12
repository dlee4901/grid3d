using TMPro;
using UnityEngine;

public class GameView : StateView
{
    public enum TimerMode { Local, Networked, Off }

    [SerializeField] private ManaCounter _manaCounter;
    [SerializeField] private InteractableUI _endTurn;
    [SerializeField] private TMP_Text _endTurnText;
    [SerializeField] private PlayerTimerView[] _playerTimerViews;
    [SerializeField] private TimerMode _timerMode = TimerMode.Local;
    [SerializeField] private int _localSeat = 1;

    private readonly PlayerTimers _playerTimers = new();
    private int _shownActivePlayer;

    protected override void Start()
    {
        base.Start();
        if (_endTurn != null) _endTurn.OnClickCompleted += EndActiveTurn;
    }

    protected override void OnGameStarted()
    {
        InitPlayerTimers();
        Refresh();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_endTurn != null) _endTurn.OnClickCompleted -= EndActiveTurn;
        _playerTimers.SeatExpired -= OnTimersExpired;
    }

    protected override void Refresh()
    {
        if (!_gridManager.IsGameStarted) return;
        var state = _gridManager.GridState;
        var players = state.Definition.PlayerCount;

        _manaCounter.SetManaCount(state.GetMana(state.ActivePlayer));

        var turnChanged = state.ActivePlayer != _shownActivePlayer;
        _shownActivePlayer = state.ActivePlayer;

        for (var p = 1; p <= players; p++)
        {
            var isActive = p == state.ActivePlayer;
            var isRunning = _timerMode != TimerMode.Off && isActive;
            if (turnChanged || !_playerTimers.IsRunning(p))
                _playerTimers.SetTimeMs(p, state.GetTimeMs(p));
            _playerTimers.SetRunning(p, isRunning);

            var view = ViewFor(p);
            if (view == null) continue;
            view.SetLabel("Player " + p);
            view.SetTimeMs(_playerTimers.RemainingMs(p));
            view.SetActiveVisual(isActive && _timerMode != TimerMode.Off);
        }

        _endTurnText.text = "END TURN\n" + state.Turn;
        if (_endTurn != null)
            _endTurn.gameObject.SetActive(CanEndTurnFor(state.ActivePlayer));
    }

    private void InitPlayerTimers()
    {
        var state = _gridManager.GridState;
        var players = state.Definition.PlayerCount;
        _playerTimers.Init(players);
        _playerTimers.SeatExpired += OnTimersExpired;
        for (var p = 1; p <= players; p++) _playerTimers.SetTimeMs(p, state.GetTimeMs(p));
    }

    private void Update()
    {
        if (!_gridManager.IsGameStarted) return;
        _playerTimers.Tick(Time.deltaTime);
        var view = ViewFor(_shownActivePlayer);
        if (view != null) view.SetTimeMs(_playerTimers.RemainingMs(_shownActivePlayer));
    }

    private PlayerTimerView ViewFor(int seat)
        => _playerTimerViews != null && seat >= 1 && seat <= _playerTimerViews.Length
            ? _playerTimerViews[seat - 1]
            : null;

    private void EndActiveTurn()
    {
        var active = _gridManager.GridState.ActivePlayer;
        if (CanEndTurnFor(active)) SubmitEndTurn(active);
    }

    private void OnTimersExpired(int player)
    {
        if (_timerMode == TimerMode.Off) return;
        if (!CanEndTurnFor(player)) return;
        if (_gridManager.GridState.ActivePlayer != player) return;
        SubmitEndTurn(player);
    }

    private void SubmitEndTurn(int player)
    {
        _gridManager.Submit(new EndTurnCommand(player, _playerTimers.ElapsedMs(player)));
    }

    private bool CanEndTurnFor(int player)
        => _timerMode != TimerMode.Networked || player == _localSeat;
}
