using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum SelectionMode { Default, MultiTarget, PathTrace }

public class GridInput : MonoBehaviour
{
    private Grid _grid;
    private Camera _camera;
    private IReadOnlyGridState _state;
    private InputAction _selectAction;
    private bool _initialized;

    private QueryContext? _hovered;
    private QueryContext? _selected;
    private readonly List<(int, int)> _multiTargets = new();
    private readonly List<(int, int)> _pathTrace = new();
    private SelectionMode _mode = SelectionMode.Default;
    private int _multiTargetMax;
    private int _pathTraceMax;
    private bool _isLocked;

    [SerializeField] private bool _debug;

    public QueryContext? Hovered => _hovered;
    public QueryContext? Selected => _selected;
    public IReadOnlyList<(int, int)> MultiTargets => _multiTargets;
    public IReadOnlyList<(int, int)> PathTrace => _pathTrace;
    public SelectionMode Mode => _mode;
    public bool IsLocked => _isLocked;
    public bool HasSelection => _selected.HasValue;

    public event Action<QueryContext?> OnHoverChanged;
    public event Action<QueryContext?> OnSelectionChanged;
    public event Action<IReadOnlyList<(int, int)>> OnMultiTargetsChanged;
    public event Action<IReadOnlyList<(int, int)>> OnPathTraceChanged;
    public event Action<SelectionMode> OnModeChanged;
    public event Action<bool> OnLockChanged;

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
        var ctx = _hovered.Value;
        switch (_mode)
        {
            case SelectionMode.Default:     Select(ctx); break;
            case SelectionMode.MultiTarget: AddMultiTarget(ctx.SourcePosition); break;
            case SelectionMode.PathTrace:   AppendPathTrace(ctx.SourcePosition); break;
        }
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
        ClearMultiTargets();
        ClearPathTrace();
        Log($"Selected: {Describe(_selected)}");
        OnSelectionChanged?.Invoke(_selected);
    }

    public void ClearSelection()
    {
        if (_isLocked) return;
        if (!_selected.HasValue) return;
        _selected = null;
        ClearMultiTargets();
        ClearPathTrace();
        Log("Selection cleared");
        OnSelectionChanged?.Invoke(_selected);
    }

    // ---- Multi-target (multi-tile click flow, allowed while locked)
    public void AddMultiTarget((int, int) position)
    {
        if (_multiTargetMax > 0 && _multiTargets.Count >= _multiTargetMax)
        {
            Log($"MultiTarget rejected (cap {_multiTargetMax}): {position}");
            return;
        }
        _multiTargets.Add(position);
        Log($"MultiTarget added: {position} ({_multiTargets.Count}/{_multiTargetMax})");
        OnMultiTargetsChanged?.Invoke(_multiTargets);
    }

    public bool RemoveMultiTarget((int, int) position)
    {
        if (!_multiTargets.Remove(position)) return false;
        Log($"MultiTarget removed: {position} ({_multiTargets.Count}/{_multiTargetMax})");
        OnMultiTargetsChanged?.Invoke(_multiTargets);
        return true;
    }

    public void ClearMultiTargets()
    {
        if (_multiTargets.Count == 0) return;
        _multiTargets.Clear();
        Log("MultiTargets cleared");
        OnMultiTargetsChanged?.Invoke(_multiTargets);
    }

    // ---- Path trace (sequence of tiles, allowed while locked)
    public void AppendPathTrace((int, int) position)
    {
        if (_pathTraceMax > 0 && _pathTrace.Count >= _pathTraceMax)
        {
            Log($"PathTrace rejected (cap {_pathTraceMax}): {position}");
            return;
        }
        _pathTrace.Add(position);
        Log($"PathTrace appended: {position} ({_pathTrace.Count}/{_pathTraceMax})");
        OnPathTraceChanged?.Invoke(_pathTrace);
    }

    public void ClearPathTrace()
    {
        if (_pathTrace.Count == 0) return;
        _pathTrace.Clear();
        Log("PathTrace cleared");
        OnPathTraceChanged?.Invoke(_pathTrace);
    }

    // ---- Mode
    public void EnterMultiTargetMode(int maxCount)
    {
        _multiTargetMax = maxCount;
        ClearMultiTargets();
        SetMode(SelectionMode.MultiTarget);
    }

    public void EnterPathTraceMode(int maxLength)
    {
        _pathTraceMax = maxLength;
        ClearPathTrace();
        SetMode(SelectionMode.PathTrace);
    }

    public void ExitMode() => SetMode(SelectionMode.Default);

    private void SetMode(SelectionMode mode)
    {
        if (_mode == mode) return;
        var previous = _mode;
        _mode = mode;
        Log($"Mode: {previous} → {mode}");
        OnModeChanged?.Invoke(_mode);
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

    public bool IsMultiTarget((int, int) position)
        => _multiTargets.Contains(position);

    public bool IsPathTraceTile((int, int) position)
        => _pathTrace.Contains(position);

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

    // ---- Debug
    private void Log(string message)
    {
        if (!_debug) return;
        Debug.Log($"[GridInput] {message}");
    }

    private static string Describe(QueryContext? ctx)
    {
        if (!ctx.HasValue) return "<none>";
        var c = ctx.Value;
        return $"{c.SourcePosition} {c.SourceEntity?.Id ?? "<empty>"}";
    }
}
