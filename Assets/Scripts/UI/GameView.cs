using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameView : StateView
{
    [SerializeField] private EntityRenderer _entityRenderer;
    [SerializeField] private CameraRig _cameraRig;
    [SerializeField] private InteractableUI _toggleHealth;
    [SerializeField] private InteractableUI _toggleDanger;
    [SerializeField] private InteractableUI _toggleTurnEnd;
    [SerializeField] private InteractableUI _togglePerspective;
    [SerializeField] private Image _healthBorder;
    [SerializeField] private Image _dangerBorder;
    [SerializeField] private Image _turnEndBorder;
    [SerializeField] private Image _perspectiveBorder;
    [SerializeField] private Color32 _toggleOnColor = new(255, 217, 0, 255);
    [SerializeField] private Color32 _toggleOffColor = new(255, 255, 255, 64);
    [SerializeField] private InteractableUI _cameraCenter;
    [SerializeField] private InteractableUI _cameraRotate;
    [SerializeField] private InteractableUI _endTurn;
    [SerializeField] private TMP_Text _endTurnText;
    [SerializeField] private PlayerTimerView[] _playerTimerViews;
    [SerializeField] private TeamPalette _teamPalette;

    private int _shownActivePlayer;
    private bool _showDanger;

    protected override void Start()
    {
        base.Start();
        if (_endTurn != null) _endTurn.OnClickCompleted += OnEndTurnClicked;
        _gridManager.Player.TargetingChanged += RenderShown;
        if (_toggleHealth != null) _toggleHealth.OnClickCompleted += ToggleHealth;
        if (_toggleDanger != null) _toggleDanger.OnClickCompleted += ToggleDanger;
        if (_toggleTurnEnd != null) _toggleTurnEnd.OnClickCompleted += ToggleTurnEnd;
        if (_togglePerspective != null) _togglePerspective.OnClickCompleted += TogglePerspective;
        _gridManager.StateChanged += RefreshDanger;
        if (_cameraCenter != null) _cameraCenter.OnClickCompleted += _cameraRig.ResetView;
        if (_cameraRotate != null) _cameraRotate.OnClickCompleted += _cameraRig.Rotate;
        ShowToggles();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_endTurn != null) _endTurn.OnClickCompleted -= OnEndTurnClicked;
        if (_gridManager != null && _gridManager.Player != null) _gridManager.Player.TargetingChanged -= RenderShown;
        if (_toggleHealth != null) _toggleHealth.OnClickCompleted -= ToggleHealth;
        if (_toggleDanger != null) _toggleDanger.OnClickCompleted -= ToggleDanger;
        if (_toggleTurnEnd != null) _toggleTurnEnd.OnClickCompleted -= ToggleTurnEnd;
        if (_togglePerspective != null) _togglePerspective.OnClickCompleted -= TogglePerspective;
        if (_gridManager != null) _gridManager.StateChanged -= RefreshDanger;
        if (_cameraCenter != null && _cameraRig != null) _cameraCenter.OnClickCompleted -= _cameraRig.ResetView;
        if (_cameraRotate != null && _cameraRig != null) _cameraRotate.OnClickCompleted -= _cameraRig.Rotate;
    }

    protected override void Render(GridState shown)
    {
        if (!_gridManager.IsGameStarted) return;
        var state = shown;
        var real = _gridManager.GridState;
        var turns = _gridManager.Turns;
        var players = state.Definition.PlayerCount;

        _shownActivePlayer = real.ActivePlayer;

        for (var p = 1; p <= players; p++)
        {
            var isActive = p == real.ActivePlayer;
            var view = ViewFor(p);
            if (view == null) continue;
            view.SetLabel("Player " + p);
            view.SetTeamColor(_teamPalette.ColorForPlayer(p, _gridManager.ViewerSeat));
            view.SetTimeMs(turns.RemainingMs(p));
            view.SetActiveVisual(isActive && turns.TimersEnabled);
        }

        if (_endTurnText != null) _endTurnText.text = EndTurnLabel(real);
        if (_endTurn != null)
        {
            _endTurn.enabled = _gridManager.Player.State is PreviewState || !_gridManager.IsPreviewing;
            _endTurn.gameObject.SetActive(real.IsGameOver || _gridManager.StopReason != null
                                          || real.ActivePlayer == _gridManager.LocalSeat);
        }
    }

    private void Update()
    {
        if (!_gridManager.IsGameStarted) return;
        var view = ViewFor(_shownActivePlayer);
        if (view != null) view.SetTimeMs(_gridManager.Turns.RemainingMs(_shownActivePlayer));
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame
            && _endTurn != null && _endTurn.isActiveAndEnabled) OnEndTurnClicked();
    }

    private string EndTurnLabel(GridState state)
    {
        if (_gridManager.Player.State is PreviewState) return _gridManager.Player.CanConfirm ? "CONFIRM" : "PREVIEW";
        if (_gridManager.IsPreviewing) return "PREVIEW";
        if (state.Outcome == GameOutcome.Draw) return "DRAW";
        if (state.Outcome == GameOutcome.Victory)
        {
            if (!_gridManager.IsNetworked) return $"PLAYER {state.Winner}\nWINS";
            return state.Winner == _gridManager.LocalSeat ? "YOU\nWIN" : "YOU\nLOSE";
        }
        if (_gridManager.StopReason != null) return "MATCH\nSTOPPED";
        return "TURN " + state.Turn;
    }

    private void OnEndTurnClicked()
    {
        var player = _gridManager.Player;
        if (player.CanConfirm) player.Confirm();
        else if (player.State is PreviewState) player.Cancel();
    }

    private PlayerTimerView ViewFor(int seat)
        => _playerTimerViews != null && seat >= 1 && seat <= _playerTimerViews.Length
            ? _playerTimerViews[seat - 1]
            : null;

    private void ToggleDanger()
    {
        _showDanger = !_showDanger;
        RefreshDanger();
        ShowToggles();
    }

    private void RefreshDanger()
    {
        if (!_gridManager.IsGameStarted) return;
        _gridManager.Renderer.Highlights.SetDanger(_showDanger ? ThreatMap.For(_gridManager.GridState, _gridManager.LocalSeat) : null);
    }

    private void ToggleTurnEnd()
    {
        _gridManager.TogglePreviewTurnEnd();
        ShowToggles();
    }

    private void TogglePerspective()
    {
        _cameraRig.ToggleTopDown();
        ShowToggles();
    }

    private void ToggleHealth()
    {
        _entityRenderer.ShowHealthCounters = !_entityRenderer.ShowHealthCounters;
        ShowToggles();
    }

    private void ShowToggles()
    {
        Tint(_healthBorder, _entityRenderer.ShowHealthCounters);
        Tint(_dangerBorder, _showDanger);
        Tint(_turnEndBorder, _gridManager.PreviewsTurnEnd);
        Tint(_perspectiveBorder, !_cameraRig.TopDown);
    }

    private void Tint(Image border, bool on)
    {
        if (border == null) return;
        border.color = on ? _toggleOnColor : _toggleOffColor;
    }
}
