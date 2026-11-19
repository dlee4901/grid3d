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

    private GridSource? _hovered;
    private bool _isLocked;

    private readonly List<RaycastResult> _uiHits = new();
    private PointerEventData _pointerData;

    [SerializeField] private float _cancelClickMaxSeconds = 1f;
    private float? _cancelPressTime;
    private bool _pressedOnGrid;

    public GridSource? Hovered => _hovered;
    public bool IsPointerOverUI { get; private set; }

    public event Action<GridSource> OnPositionPressed;
    public event Action<GridSource> OnPositionSelected;
    public event Action OnCancelClicked;
    public event Action<GridSource?> OnHoverChanged;

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

    private void Update()
    {
        if (!_initialized) return;
        IsPointerOverUI = IsPointerOverAnyUI();
        if (IsPointerOverUI)
        {
            SetHover(null);
            _cancelPressTime = null;
            _pressedOnGrid = false;
            return;
        }
        UpdateHover();
        if (_selectAction.WasPressedThisFrame()) HandlePress();
        if (_selectAction.WasReleasedThisFrame()) HandleRelease();
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

    private void UpdateHover()
    {
        UnityUtil.GetMouseWorldPosition(_camera, out var worldPos, out var error);
        if (error) { SetHover(null); return; }
        var cell = _grid.WorldToCell(worldPos);
        var gridPosition = new GridPosition(_state, (cell.x, cell.y));
        if (!gridPosition.IsValid()) { SetHover(null); return; }
        SetHover(new GridSource(_state, gridPosition, _state.GetEntity(gridPosition)));
    }

    private void HandlePress()
    {
        if (!_hovered.HasValue)
        {
            _cancelPressTime = Time.unscaledTime;
            return;
        }
        _pressedOnGrid = true;
        Log($"Press: {Describe(_hovered)}");
        OnPositionPressed?.Invoke(_hovered.Value);
    }

    private void HandleRelease()
    {
        var pressedOnGrid = _pressedOnGrid;
        _pressedOnGrid = false;
        if (pressedOnGrid && _hovered.HasValue)
        {
            Log($"Click: {Describe(_hovered)}");
            OnPositionSelected?.Invoke(_hovered.Value);
            return;
        }
        var pressTime = _cancelPressTime;
        _cancelPressTime = null;
        if (pressTime == null || _hovered.HasValue) return;
        if (Time.unscaledTime - pressTime.Value > _cancelClickMaxSeconds) return;
        Log("Click: off map");
        OnCancelClicked?.Invoke();
    }
    
    public void SetHover(GridSource? source)
    {
        if (_hovered == source) return;
        _hovered = source;
        Log($"Hover: {Describe(_hovered)}");
        OnHoverChanged?.Invoke(_hovered);
    }

    private static string Describe(GridSource? source)
    {
        if (!source.HasValue) return "<none>";
        var c = source.Value;
        return $"{c.Position} {c.Entity?.Id ?? "<empty>"}";
    }
}
