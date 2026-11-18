using UnityEngine;

public class GridRenderer : LoggableBehaviour
{
    [SerializeField] private GridManager _gridManager;

    [SerializeField] private CameraRig _cameraRig;
    [SerializeField] private Grid _grid;
    [SerializeField] private PositionCube _positionCubePrefab;
    [SerializeField] private EntityRenderer _entityRenderer;
    [SerializeField] private HighlightRenderer _highlights;
    [SerializeField] private MovePathRenderer _movePaths;

    private PositionCube[] _positionCubes;
    private float _gridGroundLevel;

    private GridState GridState => _gridManager.GridState;

    public Grid Grid => _grid;
    public HighlightRenderer Highlights => _highlights;

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
        if (_gridManager != null) _gridManager.GameStarted -= OnGameStarted;
    }

    private void OnGameStarted() => Build();

    public void Build()
    {
        if (_positionCubes != null) return;
        _grid.gameObject.SetActive(true);
        InitRendering();
        InitCamera();
        _highlights.Build(_positionCubes);
        _movePaths.Build();
        _entityRenderer.Build();
    }

    private void InitCamera()
    {
        var min = CellCenter(new GridPosition(GridState, (0, 0)));
        var max = CellCenter(new GridPosition(GridState, (GridState.X - 1, GridState.Y - 1)));
        var halfCell = new Vector3(0.5f, 0f, 0.5f);
        var bounds = new Bounds();
        bounds.SetMinMax(min - halfCell, max + halfCell);
        _cameraRig.Build(bounds);
    }

    private void InitRendering()
    {
        _gridGroundLevel = _positionCubePrefab.transform.localScale.y;
        _positionCubes = new PositionCube[GridState.Size];
        for (var x = 0; x < GridState.X; x++)
        {
            for (var y = 0; y < GridState.Y; y++)
            {
                var gridPosition = new GridPosition(GridState, (x, y));
                _positionCubes[gridPosition.Dim1] = Instantiate(_positionCubePrefab, CellCenter(gridPosition, -_gridGroundLevel / 2.0f), Quaternion.identity, gameObject.transform);
            }
        }
    }
}
