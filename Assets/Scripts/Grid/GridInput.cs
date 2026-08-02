using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GridInput : LoggableBehaviour
{
    private Grid _grid;
    private Camera _camera;
    private IReadOnlyGridState _state;
    private InputAction _selectAction;
    private bool _initialized;

    private QueryContext? _hovered;
    private bool _isLocked;

    public QueryContext? Hovered => _hovered;

    public event Action<QueryContext> OnPositionSelected;
    public event Action OnCancelClicked;
    public event Action<QueryContext?> OnHoverChanged;

    // public event Action<bool> OnLockChanged;
    // public bool IsLocked => _isLocked;

    public void Init(Grid grid, Camera camera, IReadOnlyGridState state, InputAction selectAction)
    {
        _grid = grid;
        _camera = camera;
        _state = state;
        _selectAction = selectAction;
        _initialized = true;
    }

    private void Update()
    {
        if (!_initialized) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            SetHover(null);
            return;
        }
        UpdateHover();
        if (_selectAction.triggered) HandleClick();
    }

    private void UpdateHover()
    {
        UnityUtil.GetMouseWorldPosition(_camera, out var worldPos, out var error);
        if (error) { SetHover(null); return; }
        var cell = _grid.WorldToCell(worldPos);
        var gridPosition = new GridPosition(_state, (cell.x, cell.y));
        if (!gridPosition.IsValid()) { SetHover(null); return; }
        SetHover(new QueryContext(_state, gridPosition, _state.GetEntity(gridPosition)));
    }

    private void HandleClick()
    {
        if (!_hovered.HasValue)
        {
            OnCancelClicked?.Invoke();
            return;
        }
        Log($"Click: {Describe(_hovered)}");
        OnPositionSelected?.Invoke(_hovered.Value);
    }
    
    public void SetHover(QueryContext? ctx)
    {
        if (_hovered == ctx) return;
        _hovered = ctx;
        Log($"Hover: {Describe(_hovered)}");
        OnHoverChanged?.Invoke(_hovered);
    }

    private static string Describe(QueryContext? ctx)
    {
        if (!ctx.HasValue) return "<none>";
        var c = ctx.Value;
        return $"{c.SourcePosition} {c.SourceEntity?.Id ?? "<empty>"}";
    }
}
