using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : LoggableBehaviour
{
    [SerializeField] private CinemachineCamera _cinemachineCamera;
    
    [SerializeField] private Grid _grid;
    [SerializeField] private GameObject _gridLines;
    [SerializeField] private LineRenderer _pressOutline;
    [SerializeField] private GameObject _cubePrefab;
    
    [SerializeField] private SpriteRenderer _selectionSquare;
    [SerializeField] private Color _selectionPreviewColor = new Color(255f, 255f, 255f, 64f);
    [SerializeField] private Color _selectionActiveColor = new Color(255f, 255f, 255f, 128f);
    
    [SerializeField] private List<EntityAssets> _entityAssets;
    
    private Camera _mainCamera;
    private InputAction _selectAction;
    
    private TurnExecutor _executor;
    private CommandDispatcher _dispatcher;
    public IReadOnlyGridState GridState => _executor.State;
    public TurnExecutor Executor => _executor;
    public GridInput Input { get; private set; }
    public PlayerInputController Player { get; private set; }
    private float _gridGroundLevel;

    // Fired after any command re-renders the grid; UI panels refresh from current state.
    public event Action StateChanged;

    // Frontend entry point for UI-issued commands (end turn, timer timeouts). Routes through the
    // dispatcher — the same seam networking will intercept.
    public bool Submit(ICommand command) => _dispatcher.Submit(command);

    // Set false when a lobby/relay flow will call StartGame() itself once peers are ready.
    [SerializeField] private bool _autoStartOnLoad = true;
    public bool IsGameStarted { get; private set; }
    public event Action GameStarted;

    //private MeshRenderer[] _selectionSquares;
    private SpriteRenderer[] _selectionSquares;
    private GameObject[] _squarePrefabs;
    private GameObject[] _squareBorderPrefabs;
    private GameObject[] _entityModels;

    private void Awake()
    {
        GridLog.Info    = Debug.Log;
        GridLog.Warning = Debug.LogWarning;
        GridLog.Error   = Debug.LogError;

        Input = gameObject.AddComponent<GridInput>();
        Player = gameObject.AddComponent<PlayerInputController>();
    }

    void Start()
    {
        if (_autoStartOnLoad) StartGame();
    }

    // Explicit "begin the match" entry point. State does not exist until this runs; the lobby/relay
    // flow will call it once peers are ready (set _autoStartOnLoad = false to hand it that control).
    public void StartGame()
    {
        if (IsGameStarted) return;

        _mainCamera = Camera.main;
        _selectAction = InputSystem.actions.FindAction("Player/Select");
        _grid.gameObject.SetActive(true);

        InitTestRegistry();
        InitGrid();

        InitCamera();
        InitRendering();
        InstantiateEntityModels();

        // Subscribe only after the initial render exists (LoadTeam setup commands ran in InitGrid).
        _executor.CommandApplied += OnCommandApplied;

        Input.Init(_grid, _mainCamera, GridState, _selectAction);
        Input.OnSelectionChanged += OnInputChanged;
        Player.Init(this, Input, _executor, _dispatcher);

        GridState.PrintGrid();

        IsGameStarted = true;
        GameStarted?.Invoke();               // views build their State-dependent UI now
    }

    private void Update()
    {
        // Debug-only end-turn trigger until a real end-turn UI exists.
        if (_debug && _dispatcher != null
            && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            _dispatcher.Submit(new EndTurnCommand(GridState.ActivePlayer));
            Log($"[debug] end turn -> active={GridState.ActivePlayer}, turn={GridState.Turn}, mana={GridState.GetMana(GridState.ActivePlayer)}");
        }
    }

    // Every command routes through here: re-render the grid, then let the UI refresh from state.
    private void OnCommandApplied(ICommand command)
    {
        RefreshEntityModelPositions();
        StateChanged?.Invoke();
    }

    private void OnInputChanged(QueryContext? ctx)
    {
        if (!ctx.HasValue)
        {
            _pressOutline.gameObject.SetActive(false);
            return;
        }
        var (x, y) = ctx.Value.SourcePosition;
        var worldPos = _grid.CellToWorld(new Vector3Int(x, y, 0));
        _pressOutline.transform.position = new Vector3(worldPos.x, _gridGroundLevel + 0.05f, worldPos.z);
        _pressOutline.gameObject.SetActive(true);
    }

    public Vector2 GetSize()
    {
        return new Vector2(GridState.X, GridState.Y);
    }
    
    private void InitCamera()
    {
        var targetScreenHeight = Math.Max(GridState.X / 2.0f, GridState.Y);
        if (_cinemachineCamera.Target.TrackingTarget.Equals(_gridLines.transform))
        {
            _cinemachineCamera.GetComponent<CinemachineFollow>().FollowOffset = new Vector3(0f, targetScreenHeight, 0f);
        }
    }
    
    //
    // TESTING
    //
    
    // private void InitRegistries()
    // {
    //     RegistryManager.RegisterDerivedTypes<AbilitySelection>();
    //     RegistryManager.RegisterDerivedTypes<IEntityComponent>();
    //     
    //     var mapsPath = Path.Combine(Application.streamingAssetsPath, "Content/Maps");
    //     RegistryManager.LoadAndRegister<MapConfig>(mapsPath);
    //     
    //     var abilitiesPath = Path.Combine(Application.streamingAssetsPath, "Content/Abilities");
    //     RegistryManager.LoadAndRegister<AbilityConfig>(abilitiesPath, true, true);
    //     
    //     var entitiesPath = Path.Combine(Application.streamingAssetsPath, "Content/Entities");
    //     RegistryManager.LoadAndRegister<EntityConfig>(entitiesPath, true);
    //     //RegistryManager.LoadInstancesAndRegister<EntityConfig, Entity>(entitiesPath, Entity.Create, true);
    //     
    //     RegistryManager.Register(_entityAssets);
    // }
    
    private void InitTestRegistry()
    {
        // RegistryManager.RegisterDerivedTypes<IEntityComponent>();
        
        IdRegistry<EntityConfig>.Register(UnitConfigs.IceWizard);
        IdRegistry<AbilityConfig>.Register(AbilityConfigs.IcicleBlast);
        IdRegistry<AbilityConfig>.Register(AbilityConfigs.MoveStraight3Step);
        RegistryManager.Register(_entityAssets);

        var mapsPath = Path.Combine(Application.streamingAssetsPath, "Content/Maps");
        RegistryManager.LoadAndRegister<GridDefinition>(mapsPath);
    }
    
    private void CreateTestTeams()
    {
        var team1 = new TeamData("TestTeam1", "TestMap1", new Dictionary<int, string>{ {2, "IceWizard"} });
        var team2 = new TeamData("TestTeam2", "TestMap1", new Dictionary<int, string>{ {110, "IceWizard"} });
        JsonHandler.SaveData(team1);
        JsonHandler.SaveData(team2);
    }
    
    private void LoadTestTeams()
    {
        var team1 = JsonHandler.LoadData<TeamData>("TestTeam1");
        var team2 = JsonHandler.LoadData<TeamData>("TestTeam2");
        _executor.Apply(new LoadTeamCommand(1, team1));
        _executor.Apply(new LoadTeamCommand(2, team2));
    }
    
    private void InitGrid()
    {
        if (!IdRegistry<GridDefinition>.TryGet("TestMap1", out var definition))
        {
            LogError("TestMap1 not found");
            return;
        }

        _executor = TurnExecutor.ForDefinition(definition);
        _dispatcher = new CommandDispatcher(_executor);
        
        CreateTestTeams();
        LoadTestTeams();
        
        Log(GridState.PrintGrid());
    }
    
    //
    // RENDERING
    //
    
    private void InitRendering()
    {
        _gridGroundLevel = _cubePrefab.transform.localScale.y;
        _selectionSquares = new SpriteRenderer[GridState.Size];
        _squarePrefabs = new GameObject[GridState.Size];
        for (var x = 0; x < GridState.X; x++)
        {
            for (var y = 0; y < GridState.Y; y++)
            {
                _selectionSquares[GridState.ToPosition1D(x, y)] = Instantiate(_selectionSquare, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel + 0.01f, 0.5f), Quaternion.Euler(90f, 0f, 0f), gameObject.transform);
                _squarePrefabs[GridState.ToPosition1D(x, y)] = Instantiate(_cubePrefab, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel / 2.0f, 0.5f), Quaternion.identity, gameObject.transform);
            }
        }
        
        // Border cubes
        // for (var x = -1; x <= State.X; x++)
        // {
        //     Instantiate(_cubePrefab, _grid.CellToWorld(new Vector3Int(x, -1, 0)) + new Vector3(0.5f, _gridGroundLevel / 2.0f, 0.5f), Quaternion.identity, gameObject.transform);
        //     Instantiate(_cubePrefab, _grid.CellToWorld(new Vector3Int(x, State.Y, 0)) + new Vector3(0.5f, _gridGroundLevel / 2.0f, 0.5f), Quaternion.identity, gameObject.transform);
        // }
        // for (var y = 0; y < State.Y; y++)
        // {
        //     Instantiate(_cubePrefab, _grid.CellToWorld(new Vector3Int(-1, y, 0)) + new Vector3(0.5f, _gridGroundLevel / 2.0f, 0.5f), Quaternion.identity, gameObject.transform);
        //     Instantiate(_cubePrefab, _grid.CellToWorld(new Vector3Int(State.X, y, 0)) + new Vector3(0.5f, _gridGroundLevel / 2.0f, 0.5f), Quaternion.identity, gameObject.transform);
        // }
        
        _gridLines.SetActive(true);
        _gridLines.transform.position = new Vector3(GridState.X/2.0f, _gridGroundLevel + 0.01f, GridState.Y/2.0f);
        _gridLines.transform.localScale = new Vector3(GridState.X/10f, 1, GridState.Y/10f);
        var material = _gridLines.GetComponent<MeshRenderer>().material;
        material.SetVector("_Size", new Vector2(GridState.X, GridState.Y));
    }

    private void InstantiateEntityModels()
    {
        _entityModels = new GameObject[GridState.Size];
        foreach (var position in GridState.GetOccupiedTilesPositionSet())
        {
            var entity = GridState.GetEntity(position);
            if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets)) continue;
            if (assets.Model3D == null) continue;

            var (x, y) = GridState.ToPosition2D(position);
            var worldPos = _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            var rotation = Quaternion.identity;
            if (entity.TryGetComponent<ControlComponent>(out var control) && control.PlayerController == 2)
                rotation = Quaternion.Euler(0, 180f, 0);
            _entityModels[position] = Instantiate(assets.Model3D, worldPos, rotation, gameObject.transform);
        }
    }

    public void ShowAbilityPreview(Ability ability, QueryContext ctx)
    {
        var (areas, _) = ability.Selection.GetSelectablePositions(ctx);
        ShowTiles(areas.ToHashSet(), true);
    }

    public void ClearAbilityPreview()
    {
        ShowTiles(new HashSet<int>(), true);
    }

    public void HighlightTargets(IReadOnlyList<(int, int)> targets)
    {
        ShowTiles(targets.ToHashSet(), false);
    }

    public void ClearTargetHighlight()
    {
        ShowTiles(new HashSet<int>(), false);
    }

    public void RefreshEntityModelPositions()
    {
        var entityToModel = new Dictionary<Entity, GameObject>();
        for (var i = 0; i < _entityModels.Length; i++)
        {
            if (_entityModels[i] == null) continue;
            var entity = GridState.GetEntity(i);
            if (entity == null)
            {
                Destroy(_entityModels[i]);
                _entityModels[i] = null;
                continue;
            }
            entityToModel[entity] = _entityModels[i];
            _entityModels[i] = null;
        }
        foreach (var (entity, model) in entityToModel)
        {
            var (x, y) = GridState.ToPosition2D(entity.Position);
            model.transform.position = _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            _entityModels[entity.Position] = model;
        }
        foreach (var pos in GridState.GetOccupiedTilesPositionSet())
        {
            if (_entityModels[pos] != null) continue;
            var entity = GridState.GetEntity(pos);
            if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets)) continue;
            if (assets.Model3D == null) continue;
            var (x, y) = GridState.ToPosition2D(pos);
            var worldPos = _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            var rotation = Quaternion.identity;
            if (entity.TryGetComponent<ControlComponent>(out var control) && control.PlayerController == 2)
                rotation = Quaternion.Euler(0, 180f, 0);
            _entityModels[pos] = Instantiate(assets.Model3D, worldPos, rotation, gameObject.transform);
        }
    }
    
    private void ShowTiles(HashSet<int> tiles, bool preview)
    {
        for (int i = 0; i < _selectionSquares.Length; i++) 
            _selectionSquares[i].gameObject.SetActive(false);
        foreach (var tile in tiles)
        {
            if (GridState.IsValidPosition(tile))
            {
                //var square = _selectionSquares[tile];
                _selectionSquares[tile].color = preview ? _selectionPreviewColor : _selectionActiveColor;
                _selectionSquares[tile].gameObject.SetActive(true);
            }
        }
    }
    
    private void ShowTiles(HashSet<(int, int)> tiles, bool preview)
    {
        var positions = new HashSet<int>();
        foreach (var tile in tiles)
            positions.Add(GridState.ToPosition1D(tile));
        ShowTiles(positions, preview);
    }
    
    private void RenderGrid()
    {
        for (var i = 0; i < GridState.X; i++)
        {
            for (var j = 0; j < GridState.Y; j++)
            {
                // var tileTerrain = (int)State.TileTerrain[State.ToPosition1D(i, j)];
                // if (tileTerrain < 0 || tileTerrain > _testTileTerrainVisuals.GameObjects.Count - 1)
                // {
                //     Debug.LogError("GridManager: TestTileTerrainVisuals does not have all valid TileTerrain options");
                //     return;
                // }
                // TODO: _testTileTerrainVisuals.GameObjects[tileTerrain];
            }
        }
    }
}
