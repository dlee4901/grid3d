using TMPro;
using UnityEngine;

public class GameView : StateView
{
    [SerializeField] private EntityRenderer _entityRenderer;
    [SerializeField] private ManaCounter _manaCounter;
    [SerializeField] private InteractableUI _toggleHealth;
    [SerializeField] private InteractableUI _cameraCenter;
    [SerializeField] private InteractableUI _cameraRotate;
    [SerializeField] private InteractableUI _endTurn;
    [SerializeField] private TMP_Text _endTurnText;
    [SerializeField] private PlayerTimerView[] _playerTimerViews;

    private int _shownActivePlayer;

    protected override void Start()
    {
        base.Start();
        if (_endTurn != null) _endTurn.OnClickCompleted += _gridManager.Turns.EndTurn;
        if (_toggleHealth != null) _toggleHealth.OnClickCompleted += ToggleHealth;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_endTurn != null && _gridManager != null) _endTurn.OnClickCompleted -= _gridManager.Turns.EndTurn;
        if (_toggleHealth != null) _toggleHealth.OnClickCompleted -= ToggleHealth;
    }

    protected override void Refresh()
    {
        if (!_gridManager.IsGameStarted) return;
        var state = _gridManager.GridState;
        var turns = _gridManager.Turns;
        var players = state.Definition.PlayerCount;

        _manaCounter.SetManaCount(state.GetMana(state.ActivePlayer));
        _shownActivePlayer = state.ActivePlayer;

        for (var p = 1; p <= players; p++)
        {
            var isActive = p == state.ActivePlayer;
            var view = ViewFor(p);
            if (view == null) continue;
            view.SetLabel("Player " + p);
            view.SetTimeMs(turns.RemainingMs(p));
            view.SetActiveVisual(isActive && turns.TimersEnabled);
        }

        _endTurnText.text = EndTurnLabel(state);
        if (_endTurn != null)
            _endTurn.gameObject.SetActive(state.IsGameOver || _gridManager.StopReason != null
                                          || state.ActivePlayer == _gridManager.LocalSeat);
    }

    private void Update()
    {
        if (!_gridManager.IsGameStarted) return;
        var view = ViewFor(_shownActivePlayer);
        if (view != null) view.SetTimeMs(_gridManager.Turns.RemainingMs(_shownActivePlayer));
    }

    private string EndTurnLabel(GridState state)
    {
        if (state.Outcome == GameOutcome.Draw) return "DRAW";
        if (state.Outcome == GameOutcome.Victory)
        {
            if (!_gridManager.IsNetworked) return $"PLAYER {state.Winner}\nWINS";
            return state.Winner == _gridManager.LocalSeat ? "YOU\nWIN" : "YOU\nLOSE";
        }
        if (_gridManager.StopReason != null) return "MATCH\nSTOPPED";
        return "END TURN\n" + state.Turn;
    }

    private PlayerTimerView ViewFor(int seat)
        => _playerTimerViews != null && seat >= 1 && seat <= _playerTimerViews.Length
            ? _playerTimerViews[seat - 1]
            : null;

    private void ToggleHealth()
    {
        _entityRenderer.ShowHealthCounters = !_entityRenderer.ShowHealthCounters;
    }
}
