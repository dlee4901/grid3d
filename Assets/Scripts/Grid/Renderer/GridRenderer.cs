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
    private GameObject[] _terrainProps;
    private TerrainType[] _propTerrain;
    private float _gridGroundLevel;

    private GridState GridState => _gridManager.GridState;

    public Grid Grid => _grid;
    public PositionCube PositionCubePrefab => _positionCubePrefab;
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
        if (_gridManager == null) return;
        _gridManager.GameStarted -= OnGameStarted;
        _gridManager.DisplayChanged -= RefreshTerrain;
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
        RefreshTerrain();
        _gridManager.DisplayChanged += RefreshTerrain;
    }

    private void RefreshTerrain()
    {
        var state = _gridManager.DisplayedState;
        for (var i = 0; i < _positionCubes.Length; i++)
        {
            if (_positionCubes[i] == null) continue;
            var position = new GridPosition(state, i);
            state.TryGetTerrain(position, out var terrain);
            IdRegistry<TerrainAssets>.TryGet(terrain.ToString(), out var assets);
            _positionCubes[i].SetTerrainEmission(terrain == TerrainType.Plain ? null : assets);
            if (_propTerrain[i] == terrain) continue;
            _propTerrain[i] = terrain;
            if (_terrainProps[i] != null) Destroy(_terrainProps[i]);
            _terrainProps[i] = assets != null && assets.Prop != null
                ? Instantiate(assets.Prop, CellCenter(position), Quaternion.identity, transform)
                : null;
        }
    }

    private void InitCamera()
    {
        var min = _grid.CellToWorld(Vector3Int.zero) + new Vector3(0f, _gridGroundLevel, 0f);
        var max = _grid.CellToWorld(new Vector3Int(GridState.X, GridState.Y, 0)) + new Vector3(0f, _gridGroundLevel, 0f);
        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        _cameraRig.Build(bounds);
    }

    private void InitRendering()
    {
        _gridGroundLevel = _positionCubePrefab.transform.localScale.y;
        _positionCubes = new PositionCube[GridState.Size];
        _terrainProps = new GameObject[GridState.Size];
        _propTerrain = new TerrainType[GridState.Size];
        for (var x = 0; x < GridState.X; x++)
        {
            for (var y = 0; y < GridState.Y; y++)
            {
                var gridPosition = new GridPosition(GridState, (x, y));
                if (!gridPosition.IsValid()) continue;
                _positionCubes[gridPosition.Dim1] = Instantiate(_positionCubePrefab, CellCenter(gridPosition, -_gridGroundLevel / 2.0f), Quaternion.identity, gameObject.transform);
            }
        }
    }
}
