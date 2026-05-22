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
    private QueryContext? _selected;
    private bool _isLocked;

    public QueryContext? Hovered => _hovered;
    public QueryContext? Selected => _selected;
    public bool IsLocked => _isLocked;
    public bool HasSelection => _selected.HasValue;

    public event Action<QueryContext?> OnHoverChanged;
    public event Action<QueryContext?> OnSelectionChanged;
    public event Action<bool> OnLockChanged;
    public event Action<QueryContext> OnTileClicked;

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
        if (!_state.IsValidPosition(cell.x, cell.y)) { SetHover(null); return; }
        SetHover(new QueryContext(_state, (cell.x, cell.y), _state.GetEntity(cell.x, cell.y)));
    }

    private void HandleClick()
    {
        if (!_hovered.HasValue) return;
        Log($"Click: {Describe(_hovered)}");
        OnTileClicked?.Invoke(_hovered.Value);
    }

    // ---- Hover (transient, never blocked by lock)
    public void SetHover(QueryContext? ctx)
    {
        if (NullableContextEquals(_hovered, ctx)) return;
        _hovered = ctx;
        Log($"Hover: {Describe(_hovered)}");
        OnHoverChanged?.Invoke(_hovered);
    }

    public void ClearHover() => SetHover(null);

    // ---- Selection (committed, blocked by lock)
    public void Select(QueryContext ctx)
    {
        if (_isLocked) return;
        if (_selected.HasValue && ContextEquals(_selected.Value, ctx)) return;
        _selected = ctx;
        Log($"Selected: {Describe(_selected)}");
        OnSelectionChanged?.Invoke(_selected);
    }

    public void ClearSelection()
    {
        if (_isLocked) return;
        if (!_selected.HasValue) return;
        _selected = null;
        Log("Selection cleared");
        OnSelectionChanged?.Invoke(_selected);
    }

    // ---- Lock (cross-turn, idempotent)
    public void Lock()
    {
        if (_isLocked) return;
        _isLocked = true;
        Log("Locked");
        OnLockChanged?.Invoke(true);
    }

    public void Unlock()
    {
        if (!_isLocked) return;
        _isLocked = false;
        Log("Unlocked");
        OnLockChanged?.Invoke(false);
    }

    // ---- Convenience predicates for UI binding
    public bool IsHovered((int, int) position)
        => _hovered.HasValue && _hovered.Value.SourcePosition == position;

    public bool IsSelected((int, int) position)
        => _selected.HasValue && _selected.Value.SourcePosition == position;

    // ---- Equality helpers
    private static bool ContextEquals(QueryContext a, QueryContext b)
        => a.SourcePosition == b.SourcePosition
           && ReferenceEquals(a.SourceEntity, b.SourceEntity)
           && ReferenceEquals(a.Grid, b.Grid);

    private static bool NullableContextEquals(QueryContext? a, QueryContext? b)
    {
        if (!a.HasValue && !b.HasValue) return true;
        if (a.HasValue != b.HasValue) return false;
        return ContextEquals(a.Value, b.Value);
    }

    private static string Describe(QueryContext? ctx)
    {
        if (!ctx.HasValue) return "<none>";
        var c = ctx.Value;
        return $"{c.SourcePosition} {c.SourceEntity?.Id ?? "<empty>"}";
    }
}
