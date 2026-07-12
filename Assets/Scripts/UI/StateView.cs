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
        Refresh();                                   // initial paint
    }

    protected virtual void OnDestroy()
    {
        if (_gridManager != null) _gridManager.StateChanged -= Refresh;
    }

    /// Re-render from current game state. Runs on Start and after every command.
    protected abstract void Refresh();
}
