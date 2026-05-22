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
    [SerializeField] private MeshRenderer _selectionSquare;
    [SerializeField] private GameObject _squarePrefab;
     
    [SerializeField] private List<EntityAssets> _entityAssets;
    
    private Camera _mainCamera;
    private InputAction _selectAction;
    
    private TurnExecutor _executor;
    public IReadOnlyGridState State => _executor.State;
    public TurnExecutor Executor => _executor;
    public GridInput Input { get; private set; }
    public PlayerInputController Player { get; private set; }
    private float _gridGroundLevel;

    private MeshRenderer[] _selectionSquares;
    private GameObject[] _squarePrefabs;
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
        _mainCamera = Camera.main;
        _selectAction = InputSystem.actions.FindAction("Player/Select");
        _grid.gameObject.SetActive(true);

        InitTestRegistry();
        InitGrid();

        InitCamera();
        InitRendering();
        InstantiateEntityModels();

        Input.Init(_grid, _mainCamera, State, _selectAction);
        Input.OnSelectionChanged += OnInputChanged;
        Player.Init(this, Input, _executor);

        State.PrintGrid();
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
        return new Vector2(State.X, State.Y);
    }
    
    private void InitCamera()
    {
        var targetScreenHeight = Math.Max(State.X / 2.0f, State.Y);
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
    //     RegistryManager.RegisterDerivedTypes<SkillSelection>();
    //     RegistryManager.RegisterDerivedTypes<IEntityComponent>();
    //     
    //     var mapsPath = Path.Combine(Application.streamingAssetsPath, "Content/Maps");
    //     RegistryManager.LoadAndRegister<MapConfig>(mapsPath);
    //     
    //     var skillsPath = Path.Combine(Application.streamingAssetsPath, "Content/Skills");
    //     RegistryManager.LoadAndRegister<SkillConfig>(skillsPath, true, true);
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
        IdRegistry<SkillConfig>.Register(SkillConfigs.IcicleBlast);
        IdRegistry<SkillConfig>.Register(SkillConfigs.MoveStraight3Step);
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
        
        CreateTestTeams();
        LoadTestTeams();
        
        Log(State.PrintGrid());
    }
    
    //
    // RENDERING
    //
    
    private void InitRendering()
    {
        _gridGroundLevel = _squarePrefab.transform.localScale.y;
        _selectionSquares = new MeshRenderer[State.Size];
        _squarePrefabs = new GameObject[State.Size];
        for (var x = 0; x < State.X; x++)
        {
            for (var y = 0; y < State.Y; y++)
            {
                _selectionSquares[State.ToPosition1D(x, y)] = Instantiate(_selectionSquare, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel + 0.01f, 0.5f),  Quaternion.identity, gameObject.transform);
                _squarePrefabs[State.ToPosition1D(x, y)] = Instantiate(_squarePrefab, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel / 2.0f, 0.5f), Quaternion.identity, gameObject.transform);
            }
        }
        _gridLines.SetActive(true);
        _gridLines.transform.position = new Vector3(State.X/2.0f, _gridGroundLevel + 0.01f, State.Y/2.0f);
        _gridLines.transform.localScale = new Vector3(State.X/10f, 1, State.Y/10f);
    }

    private void InstantiateEntityModels()
    {
        _entityModels = new GameObject[State.Size];
        foreach (var position in State.GetOccupiedTilesPositionSet())
        {
            var entity = State.GetEntity(position);
            if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets)) continue;
            if (assets.Model3D == null) continue;

            var (x, y) = State.ToPosition2D(position);
            var worldPos = _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            var rotation = Quaternion.identity;
            if (entity.TryGetComponent<ControlComponent>(out var control) && control.PlayerController == 2)
                rotation = Quaternion.Euler(0, 180f, 0);
            _entityModels[position] = Instantiate(assets.Model3D, worldPos, rotation, gameObject.transform);
        }
    }

    public void ShowSkillPreview(Skill skill, QueryContext ctx)
    {
        var (areas, _) = skill.Selection.GetSelectablePositions(ctx);
        ShowTiles(areas.ToHashSet());
    }

    public void ClearSkillPreview()
    {
        ShowTiles(new HashSet<int>());
    }

    public void HighlightTargets(IReadOnlyList<(int, int)> targets)
    {
        ShowTiles(targets.ToHashSet());
    }

    public void ClearTargetHighlight()
    {
        ShowTiles(new HashSet<int>());
    }

    public void RefreshEntityModelPositions()
    {
        var entityToModel = new Dictionary<Entity, GameObject>();
        for (var i = 0; i < _entityModels.Length; i++)
        {
            if (_entityModels[i] == null) continue;
            var entity = State.GetEntity(i);
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
            var (x, y) = State.ToPosition2D(entity.Position);
            model.transform.position = _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            _entityModels[entity.Position] = model;
        }
        foreach (var pos in State.GetOccupiedTilesPositionSet())
        {
            if (_entityModels[pos] != null) continue;
            var entity = State.GetEntity(pos);
            if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets)) continue;
            if (assets.Model3D == null) continue;
            var (x, y) = State.ToPosition2D(pos);
            var worldPos = _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel, 0.5f);
            var rotation = Quaternion.identity;
            if (entity.TryGetComponent<ControlComponent>(out var control) && control.PlayerController == 2)
                rotation = Quaternion.Euler(0, 180f, 0);
            _entityModels[pos] = Instantiate(assets.Model3D, worldPos, rotation, gameObject.transform);
        }
    }

    private void ShowTiles(HashSet<int> tiles)
    {
        for (int i = 0; i < _selectionSquares.Length; i++) 
            _selectionSquares[i].gameObject.SetActive(false);
        foreach (var tile in tiles)
            if (State.IsValidPosition(tile))
                 _selectionSquares[tile].gameObject.SetActive(true);
    }
    
    private void ShowTiles(HashSet<(int, int)> tiles)
    {
        var positions = new HashSet<int>();
        foreach (var tile in tiles)
            positions.Add(State.ToPosition1D(tile));
        ShowTiles(positions);
    }
    
    private void RenderGrid()
    {
        for (var i = 0; i < State.X; i++)
        {
            for (var j = 0; j < State.Y; j++)
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
