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
    
    private Grid2D _grid2D;
    private float _gridGroundLevel;
    
    private MeshRenderer[] _selectionSquares;
    private GameObject[] _squarePrefabs;
    
    void Start()
    {
        _mainCamera = Camera.main;
        _selectAction = InputSystem.actions.FindAction("Player/Select");
        _grid.gameObject.SetActive(true);
        
        InitTest();
        InitGrid();
        
        InitCamera();
        InitRendering();
    }

    void Update()
    {
        if (_selectAction.triggered) HandleMousePosition();
    }

    public Vector2 GetSize()
    {
        return new Vector2(_grid2D.X, _grid2D.Y);
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
        if (gridPosition.x >= 0 && gridPosition.x < _grid2D.X && gridPosition.y >= 0 && gridPosition.y < _grid2D.Y)
        {
            // ShowTiles(
            //     _testTileSelector.GetTileSet(_grid2D, (gridPosition.x, gridPosition.y))
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
        var entity = _grid2D.GetEntity(x, y);
        if (entity != null)
        {
            entity.TryGetComponent<SkillComponent>(out var skillComponent);
            var skill = skillComponent.List[0];
            //skill.Selection.GetRange(_grid2D, (x, y), entity);
            var ctx = new QueryContext(_grid2D.State, (x, y), entity);
            var selectablePositions = skill.Selection.GetSelectablePositions(ctx);
            ShowTiles(selectablePositions.areas.ToHashSet());
        }
    }
    
    private void InitCamera()
    {
        var targetScreenHeight = Math.Max(_grid2D.X / 2.0f, _grid2D.Y);
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
    
    private void InitTest()
    {
        // RegistryManager.RegisterDerivedTypes<IEntityComponent>();
        
        IdRegistry<EntityConfig>.Register(UnitConfigs.IceWizard);
        IdRegistry<SkillConfig>.Register(SkillConfigs.IcicleBlast);
        IdRegistry<SkillConfig>.Register(SkillConfigs.MoveStraight3Step);
        
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
        _grid2D.LoadPlayerTeam(1, team1);
        _grid2D.LoadPlayerTeam(2, team2);
    }
    
    private void InitGrid()
    {
        if (!IdRegistry<GridDefinition>.TryGet("TestMap1", out var definition))
        {
            Debug.LogError("TestMap1 not found");
            return;
        }

        _grid2D = Grid2D.Create(definition);
        
        CreateTestTeams();
        LoadTestTeams();
        
        Debug.Log(_grid2D.PrintGrid());
    }
    
    //
    // RENDERING
    //
    
    private void InitRendering()
    {
        _gridGroundLevel = _squarePrefab.transform.localScale.y;
        _selectionSquares = new MeshRenderer[_grid2D.GetSize()];
        _squarePrefabs = new GameObject[_grid2D.GetSize()];
        for (var x = 0; x < _grid2D.X; x++)
        {
            for (var y = 0; y < _grid2D.Y; y++)
            {
                _selectionSquares[_grid2D.ToPosition1D(x, y)] = Instantiate(_selectionSquare, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel + 0.01f, 0.5f),  Quaternion.identity);
                _squarePrefabs[_grid2D.ToPosition1D(x, y)] = Instantiate(_squarePrefab, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel / 2.0f, 0.5f), Quaternion.identity);
            }
        }
        _gridLines.SetActive(true);
        _gridLines.transform.position = new Vector3(_grid2D.X/2.0f, _gridGroundLevel + 0.01f, _grid2D.Y/2.0f);
        _gridLines.transform.localScale = new Vector3(_grid2D.X/10f, 1, _grid2D.Y/10f);
    }
    
    private void ShowTiles(HashSet<int> tiles)
    {
        for (int i = 0; i < _selectionSquares.Length; i++) 
            _selectionSquares[i].gameObject.SetActive(false);
        foreach (var tile in tiles)
            if (_grid2D.IsValidPosition(tile))
                 _selectionSquares[tile].gameObject.SetActive(true);
    }
    
    private void ShowTiles(HashSet<(int, int)> tiles)
    {
        var positions = new HashSet<int>();
        foreach (var tile in tiles)
            positions.Add(_grid2D.ToPosition1D(tile));
        ShowTiles(positions);
    }
    
    private void RenderGrid()
    {
        for (var i = 0; i < _grid2D.X; i++)
        {
            for (var j = 0; j < _grid2D.Y; j++)
            {
                // var tileTerrain = (int)_grid2D.TileTerrain[_grid2D.ToPosition1D(i, j)];
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
