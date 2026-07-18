using UnityEngine;

public abstract class StateView : LoggableBehaviour
{
    [SerializeField] protected GridManager _gridManager;

    protected virtual void Start()
    {
        _gridManager.StateChanged += Refresh;
        _gridManager.GameStarted += OnGameStarted;
        if (_gridManager.IsGameStarted) OnGameStarted();
    }

    protected virtual void OnDestroy()
    {
        if (_gridManager == null) return;
        _gridManager.StateChanged -= Refresh;
        _gridManager.GameStarted -= OnGameStarted;
    }

    protected virtual void OnGameStarted() => Refresh();

    protected abstract void Refresh();
}
