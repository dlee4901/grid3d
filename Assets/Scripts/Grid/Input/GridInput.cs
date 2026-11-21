using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class GridInput : LoggableBehaviour
{
    private Grid _grid;
    private Camera _camera;
    private GridState _state;
    private InputAction _selectAction;
    private bool _initialized;

    private bool _isLocked;

    private readonly List<RaycastResult> _uiHits = new();
    private PointerEventData _pointerData;

    [SerializeField] private float _cancelClickMaxSeconds = 1f;
    private float? _cancelPressTime;
    private bool _pressedOnGrid;
    private GridPosition? _dragPosition;

    public bool IsPointerOverUI { get; private set; }

    public event Action<GridSource> OnPositionPressed;
    public event Action<GridSource> OnPositionSelected;
    public event Action<GridSource> OnPositionDragged;
    public event Action OnCancelClicked;
    public event Action OnReleasedOffGrid;

    // public event Action<bool> OnLockChanged;
    // public bool IsLocked => _isLocked;

    public void Init(Grid grid, Camera camera, GridState state, InputAction selectAction)
    {
        _grid = grid;
        _camera = camera;
        _state = state;
        _selectAction = selectAction;
        _initialized = true;
    }

    public void SetState(GridState state) => _state = state;

    private void Update()
    {
        if (!_initialized) return;
        IsPointerOverUI = IsPointerOverAnyUI();
        if (IsPointerOverUI)
        {
            _cancelPressTime = null;
            if (_selectAction.WasReleasedThisFrame()) HandleRelease(null);
            return;
        }
        if (_selectAction.WasPressedThisFrame()) HandlePress(PointerCell());
        else if (_pressedOnGrid && _selectAction.IsPressed()) HandleDrag(PointerCell());
        if (_selectAction.WasReleasedThisFrame()) HandleRelease(PointerCell());
    }

    private void HandleDrag(GridSource? cell)
    {
        if (!cell.HasValue || cell.Value.Position.Equals(_dragPosition)) return;
        _dragPosition = cell.Value.Position;
        OnPositionDragged?.Invoke(cell.Value);
    }

    private bool IsPointerOverAnyUI()
    {
        var eventSystem = EventSystem.current;
        if (eventSystem == null || Mouse.current == null) return false;
        _pointerData ??= new PointerEventData(eventSystem);
        _pointerData.position = Mouse.current.position.ReadValue();
        _uiHits.Clear();
        eventSystem.RaycastAll(_pointerData, _uiHits);
        return _uiHits.Count > 0;
    }

    public GridSource? CellUnderPointer() => IsPointerOverAnyUI() ? null : PointerCell();

    private GridSource? PointerCell()
    {
        UnityUtil.GetMouseWorldPosition(_camera, out var worldPos, out var error);
        if (error) return null;
        var cell = _grid.WorldToCell(worldPos);
        var gridPosition = new GridPosition(_state, (cell.x, cell.y));
        if (!gridPosition.IsValid()) return null;
        return new GridSource(_state, gridPosition, _state.GetEntity(gridPosition));
    }

    private void HandlePress(GridSource? cell)
    {
        if (!cell.HasValue)
        {
            _cancelPressTime = Time.unscaledTime;
            return;
        }
        _pressedOnGrid = true;
        _dragPosition = cell.Value.Position;
        Log($"Press: {Describe(cell)}");
        OnPositionPressed?.Invoke(cell.Value);
    }

    private void HandleRelease(GridSource? cell)
    {
        var pressedOnGrid = _pressedOnGrid;
        _pressedOnGrid = false;
        if (pressedOnGrid)
        {
            if (cell.HasValue)
            {
                Log($"Click: {Describe(cell)}");
                OnPositionSelected?.Invoke(cell.Value);
            }
            else
            {
                Log("Release: off map");
                OnReleasedOffGrid?.Invoke();
            }
            return;
        }
        var pressTime = _cancelPressTime;
        _cancelPressTime = null;
        if (pressTime == null || cell.HasValue) return;
        if (Time.unscaledTime - pressTime.Value > _cancelClickMaxSeconds) return;
        Log("Click: off map");
        OnCancelClicked?.Invoke();
    }

    private static string Describe(GridSource? source)
    {
        if (!source.HasValue) return "<none>";
        var c = source.Value;
        return $"{c.Position} {c.Entity?.Id ?? "<empty>"}";
    }
}
