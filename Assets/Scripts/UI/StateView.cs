using UnityEngine;

/// Base for top-level UI panels that re-render when game state changes.
/// Owns the GridManager.StateChanged subscription so subclasses only implement Refresh().
/// NOT for leaf widgets (AbilityIcon, ManaCounter, Outline) — those are pull-driven.
public abstract class StateView : LoggableBehaviour
{
    [SerializeField] protected GridManager _gridManager;

    protected virtual void Start()
    {
        _gridManager.StateChanged += Refresh;
        _gridManager.GameStarted += OnGameStarted;
        if (_gridManager.IsGameStarted) OnGameStarted();   // sticky: match already started → init now
    }

    protected virtual void OnDestroy()
    {
        if (_gridManager == null) return;
        _gridManager.StateChanged -= Refresh;
        _gridManager.GameStarted -= OnGameStarted;
    }

    // Runs once when the match starts (State guaranteed to exist). Subclasses that build State-dependent
    // children (e.g. GameView's timers) override this; default just paints.
    protected virtual void OnGameStarted() => Refresh();

    /// Re-render from current game state. Runs after every command (and once at game start).
    protected abstract void Refresh();
}
