using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
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
    private float _gridGroundLevel;
    
    private MeshRenderer[] _selectionSquares;
    private GameObject[] _squarePrefabs;
    private GameObject[] _entityModels;

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

        State.PrintGrid();
    }

    void Update()
    {
        if (_selectAction.triggered) HandleMousePosition();
    }

    public Vector2 GetSize()
    {
        return new Vector2(State.X, State.Y);
    }
    
    private void HandleMousePosition()
    {
        UnityUtil.GetMouseWorldPosition(_mainCamera, out var mouseWorldPosition, out var error);
        if (error)
        {
            _pressOutline.gameObject.SetActive(false);
            return;
        }
        Vector3Int gridPosition = _grid.WorldToCell(mouseWorldPosition);
        if (gridPosition.x >= 0 && gridPosition.x < State.X && gridPosition.y >= 0 && gridPosition.y < State.Y)
        {
            // ShowTiles(
            //     _testTileSelector.GetTileSet(_state, (gridPosition.x, gridPosition.y))
            //     );
            _pressOutline.gameObject.SetActive(true);
            var worldPos = _grid.CellToWorld(gridPosition);
            _pressOutline.transform.position = new Vector3(worldPos.x, _gridGroundLevel + 0.05f, worldPos.z);
            HandleEntityTest(gridPosition.x, gridPosition.y);
        }
        else
        {
            _pressOutline.gameObject.SetActive(false);
        }
    }
    
    private void HandleEntityTest(int x, int y)
    {
        var entity = State.GetEntity(x, y);
        var ctx = new QueryContext(State, (x, y), entity);
        SelectionController.Singleton.Select(ctx);

        if (entity != null)
        {
            entity.TryGetComponent<SkillComponent>(out var skillComponent);
            var skill = skillComponent.List[0];
            //skill.Selection.GetRange(_state, (x, y), entity);
            var selectablePositions = skill.Selection.GetSelectablePositions(ctx);
            ShowTiles(selectablePositions.areas.ToHashSet());
        }
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
            Debug.LogError("TestMap1 not found");
            return;
        }

        _executor = TurnExecutor.ForDefinition(definition);
        
        CreateTestTeams();
        LoadTestTeams();
        
        Debug.Log(State.PrintGrid());
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
