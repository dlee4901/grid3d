using System;
using System.Collections.Generic;
using UnityEngine;

public class HighlightRenderer : LoggableBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GridRenderer _gridRenderer;
    [SerializeField] private List<Sprite> _directionalArrowSprites;
    [SerializeField] private Color32 _directionChevronColor = new(255, 255, 255, 90);
    [SerializeField] private Material _projectileMaterial;
    [SerializeField] private float _projectileWidth = 0.08f;

    private PositionCube[] _positionCubes;
    private SpriteRenderer[] _directionalArrows;
    private LinePool _projectiles;

    private readonly Dictionary<int, HighlightType> _highlights = new();
    private int? _selectedIndex;
    private readonly HashSet<int> _danger = new();

    private const float DirectionalArrowHeight = 0.02f;
    private const float ProjectileHeight = 0.05f;

    public event Action HighlightsChanged;

    public void Build(PositionCube[] positionCubes)
    {
        _positionCubes = positionCubes;
        var state = _gridManager.GridState;
        _directionalArrows = new SpriteRenderer[state.Size];
        for (var x = 0; x < state.X; x++)
        {
            for (var y = 0; y < state.Y; y++)
            {
                var position = new GridPosition(state, (x, y));
                if (!position.IsValid()) continue;
                var arrow = new GameObject("DirectionalArrow").AddComponent<SpriteRenderer>();
                arrow.transform.SetParent(transform);
                arrow.transform.SetPositionAndRotation(_gridRenderer.CellCenter(position, DirectionalArrowHeight), Quaternion.Euler(90f, 0f, 0f));
                arrow.gameObject.SetActive(false);
                _directionalArrows[position.Dim1] = arrow;
            }
        }
        _projectiles = new LinePool(transform, _projectileMaterial);
    }

    public void SetDanger(IEnumerable<GridPosition> positions)
    {
        _danger.Clear();
        if (positions != null)
            foreach (var position in positions) _danger.Add(position.Dim1);
        for (var i = 0; i < _positionCubes.Length; i++)
            if (_positionCubes[i] != null) _positionCubes[i].SetDanger(_danger.Contains(i));
    }

    public void SetSelected(GridPosition? position)
    {
        var previous = _selectedIndex;
        _selectedIndex = position?.Dim1;
        if (previous == _selectedIndex) return;
        RefreshCube(previous);
        RefreshCube(_selectedIndex);
    }

    private void RefreshCube(int? index)
    {
        if (!index.HasValue) return;
        var cube = _positionCubes[index.Value];
        if (_highlights.TryGetValue(index.Value, out var type)) cube.SetHighlight(type);
        else if (index == _selectedIndex) cube.SetHighlight(HighlightType.Selected);
        else cube.ClearHighlight();
    }

    public bool TryGetHighlight(GridPosition position, out HighlightType type)
        => _highlights.TryGetValue(position.Dim1, out type);

    public void Clear()
    {
        foreach (var cube in _positionCubes)
            if (cube != null) cube.ClearHighlight();
        foreach (var arrow in _directionalArrows)
            if (arrow != null) arrow.gameObject.SetActive(false);
        _projectiles.HideAll();
        _highlights.Clear();
        RefreshCube(_selectedIndex);
        HighlightsChanged?.Invoke();
    }

    public void Highlight(HashSet<GridPosition> positions, HighlightType type)
    {
        foreach (var position in positions) SetHighlight(position, type);
        HighlightsChanged?.Invoke();
    }

    public void Highlight(GridSteps steps, HighlightType type)
    {
        if (_debug)
        {
            HighlightSteps(steps, type);
            return;
        }
        foreach (var position in steps.GetPositions()) SetHighlight(position, type);
        HighlightsChanged?.Invoke();
    }

    public void Projectile(GridPosition from, GridPosition to)
    {
        var points = new List<Vector3> { _gridRenderer.CellCenter(from, ProjectileHeight), _gridRenderer.CellCenter(to, ProjectileHeight) };
        _projectiles.Show(points, _projectileWidth);
    }

    public void DirectionChevrons(GridSteps steps)
    {
        foreach (var step in steps.GetSteps()) ShowDirectionalArrow(step, _directionChevronColor);
    }

    private void HighlightSteps(GridSteps gridSteps, HighlightType type)
    {
        foreach (var step in gridSteps.GetSteps())
        {
            SetHighlight(step.Position, type);
            ShowDirectionalArrow(step, Color.white);
        }
        HighlightsChanged?.Invoke();
    }

    private void ShowDirectionalArrow(GridStep step, Color color)
    {
        var arrow = _directionalArrows[step.Position.Dim1];
        arrow.sprite = _directionalArrowSprites[(int)step.Direction];
        arrow.color = color;
        arrow.gameObject.SetActive(true);
    }

    private void SetHighlight(GridPosition position, HighlightType type)
    {
        _positionCubes[position.Dim1].SetHighlight(type);
        _highlights[position.Dim1] = type;
    }
}
