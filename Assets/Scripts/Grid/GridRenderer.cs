using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class GridRenderer : LoggableBehaviour
{
    [SerializeField] private GridManager _gridManager;

    [SerializeField] private CinemachineCamera _cinemachineCamera;
    [SerializeField] private Grid _grid;
    [SerializeField] private LineRenderer _pressOutline;
    [SerializeField] private PositionCube _positionCubePrefab;
    [SerializeField] private EntityRenderer _entityRenderer;
    [SerializeField] private List<Sprite> _directionalArrowSprites;

    private PositionCube[] _positionCubes;
    private SpriteRenderer[] _directionalArrows;

    private readonly Dictionary<int, HighlightType> _highlights = new();

    private float _gridGroundLevel;

    private IReadOnlyGridState GridState => _gridManager.GridState;

    private const float UnidirectionalArrowPositionOffset = 0.02f;

    public Grid Grid => _grid;

    public event Action HighlightsChanged;

    public Vector3 CellCenter(GridPosition position, float heightOffset = 0f)
        => _grid.CellToWorld(new Vector3Int(position.Dim2.x, position.Dim2.y, 0))
           + new Vector3(0.5f, _gridGroundLevel + heightOffset, 0.5f);

    private void Start()
    {
        _gridManager.GameStarted += OnGameStarted;
        if (_gridManager.IsGameStarted) OnGameStarted();
    }

    private void OnDestroy()
    {
        if (_gridManager == null) return;
        _gridManager.GameStarted -= OnGameStarted;
        if (_gridManager.Player != null) _gridManager.Player.SelectionChanged -= MovePressOutline;
    }

    private void OnGameStarted() => Build();

    public void Build()
    {
        if (_positionCubes != null) return;
        _grid.gameObject.SetActive(true);
        InitCamera();
        InitRendering();
        _entityRenderer.Build();
        _gridManager.Player.SelectionChanged += MovePressOutline;
    }

    private void MovePressOutline(GridSource? source)
    {
        if (!source.HasValue)
        {
            _pressOutline.gameObject.SetActive(false);
            return;
        }
        var position = source.Value.Position;
        var worldPos = _grid.CellToWorld(new Vector3Int(position.Dim2.x, position.Dim2.y, 0));
        _pressOutline.transform.position = new Vector3(worldPos.x, _gridGroundLevel + 0.05f, worldPos.z);
        _pressOutline.gameObject.SetActive(true);
    }

    private void InitCamera()
    {
        var targetScreenHeight = Math.Max(GridState.X / 2.0f, GridState.Y);
        // if (_cinemachineCamera.Target.TrackingTarget.Equals(_gridLines.transform))
        // {
        //     _cinemachineCamera.GetComponent<CinemachineFollow>().FollowOffset = new Vector3(0f, targetScreenHeight, 0f);
        // }
    }

    private void InitRendering()
    {
        _gridGroundLevel = _positionCubePrefab.transform.localScale.y;
        _positionCubes = new PositionCube[GridState.Size];
        _directionalArrows = new SpriteRenderer[GridState.Size];

        for (var x = 0; x < GridState.X; x++)
        {
            for (var y = 0; y < GridState.Y; y++)
            {
                var gridPosition = new GridPosition(GridState, (x, y));
                _positionCubes[gridPosition.Dim1] = Instantiate(_positionCubePrefab, CellCenter(gridPosition, -_gridGroundLevel / 2.0f), Quaternion.identity, gameObject.transform);
                var directionalArrow = new GameObject("UnidirectionalArrow").AddComponent<SpriteRenderer>();
                directionalArrow.transform.SetParent(gameObject.transform);
                directionalArrow.transform.SetPositionAndRotation(CellCenter(gridPosition, UnidirectionalArrowPositionOffset), Quaternion.Euler(90f, 0f, 0f));
                _directionalArrows[gridPosition.Dim1] = directionalArrow;
            }
        }
    }

    public bool TryGetHighlight(GridPosition position, out HighlightType type)
        => _highlights.TryGetValue(position.Dim1, out type);

    public void ClearHighlights()
    {
        foreach (var cube in _positionCubes) cube.ClearHighlight();
        foreach (var arrow in _directionalArrows) arrow.gameObject.SetActive(false);
        _highlights.Clear();
        HighlightsChanged?.Invoke();
    }

    public void HighlightPositions(HashSet<GridPosition> positions, HighlightType type)
    {
        foreach (var position in positions) SetHighlight(position, type);
        HighlightsChanged?.Invoke();
    }

    public void HighlightPositions(GridSteps steps, HighlightType type)
    {
        if (_debug)
        {
            HighlightSteps(steps, type);
            return;
        }
        foreach (var position in steps.GetPositions()) SetHighlight(position, type);
        HighlightsChanged?.Invoke();
    }

    private void HighlightSteps(GridSteps gridSteps, HighlightType type)
    {
        foreach (var step in gridSteps.GetSteps())
        {
            var position = step.Position;
            SetHighlight(position, type);

            var direction = (int)step.Direction;
            _directionalArrows[position.Dim1].gameObject.SetActive(true);
            _directionalArrows[position.Dim1].sprite = _directionalArrowSprites[direction];
        }
        HighlightsChanged?.Invoke();
    }

    private void SetHighlight(GridPosition position, HighlightType type)
    {
        _positionCubes[position.Dim1].SetHighlight(type);
        _highlights[position.Dim1] = type;
    }
}
