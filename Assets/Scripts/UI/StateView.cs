using UnityEngine;

public abstract class StateView : LoggableBehaviour
{
    [SerializeField] protected GridManager _gridManager;

    protected virtual void Start()
    {
        _gridManager.DisplayChanged += RenderShown;
        _gridManager.GameStarted += OnGameStarted;
        if (_gridManager.IsGameStarted) OnGameStarted();
    }

    protected virtual void OnDestroy()
    {
        if (_gridManager == null) return;
        _gridManager.DisplayChanged -= RenderShown;
        _gridManager.GameStarted -= OnGameStarted;
    }

    protected virtual void OnGameStarted() => RenderShown();

    protected void RenderShown() => Render(_gridManager.DisplayedState);

    protected abstract void Render(GridState shown);
}
