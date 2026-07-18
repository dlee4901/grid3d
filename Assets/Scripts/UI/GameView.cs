using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameView : StateView
{
    public enum TimerMode { Local, Networked, Off }

    [SerializeField] private ManaCounter _manaCounter;
    [SerializeField] private InteractableUI _endTurn;
    [SerializeField] private TMP_Text _endTurnText;
    [SerializeField] private VerticalLayoutGroup _container;
    [SerializeField] private PlayerTimer _playerTimerPrefab;
    [SerializeField] private TimerMode _timerMode = TimerMode.Local;
    [SerializeField] private int _localSeat = 1;

    private PlayerTimer[] _timers;
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
    }

    protected override void Refresh()
    {
        if (_timers == null) return;
        var state = _gridManager.GridState;
        var players = state.Definition.PlayerCount;

        _manaCounter.SetManaCount(state.GetMana(state.ActivePlayer));

        var turnChanged = state.ActivePlayer != _shownActivePlayer;
        _shownActivePlayer = state.ActivePlayer;

        for (var p = 1; p <= players; p++)
        {
            var isActivePlayer = p == state.ActivePlayer;
            var isRunning = _timerMode != TimerMode.Off && isActivePlayer;
            if (turnChanged || !_timers[p].IsRunning)
                _timers[p].SetTimeMs(state.GetTimeMs(p));
            _timers[p].SetRunning(isRunning);
            _timers[p].SetActiveVisual(isActivePlayer);
        }

        _endTurnText.text = "END TURN " + state.Turn;
        if (_endTurn != null)
            _endTurn.gameObject.SetActive(CanEndTurnFor(state.ActivePlayer));
    }

    private void InitPlayerTimers()
    {
        var definition = _gridManager.GridState.Definition;
        _timers = new PlayerTimer[definition.PlayerCount + 1];
        for (var p = 1; p <= definition.PlayerCount; p++)
        {
            var timer = Instantiate(_playerTimerPrefab, _container.transform, true);
            timer.SetLabel("Player " + p);
            timer.SetTimeMs(_gridManager.GridState.GetTimeMs(p));
            var seat = p;
            timer.Expired += () => OnTimerExpired(seat);
            _timers[p] = timer;
        }
    }

    private void EndActiveTurn()
    {
        var active = _gridManager.GridState.ActivePlayer;
        if (CanEndTurnFor(active)) SubmitEndTurn(active);
    }

    private void OnTimerExpired(int player)
    {
        if (_timerMode == TimerMode.Off) return;
        if (!CanEndTurnFor(player)) return;
        if (_gridManager.GridState.ActivePlayer != player) return;
        SubmitEndTurn(player);
    }

    private void SubmitEndTurn(int player)
    {
        var elapsed = _timers[player] != null ? _timers[player].ElapsedMs : 0;
        _gridManager.Submit(new EndTurnCommand(player, elapsed));
    }

    private bool CanEndTurnFor(int player)
        => _timerMode != TimerMode.Networked || player == _localSeat;
}
