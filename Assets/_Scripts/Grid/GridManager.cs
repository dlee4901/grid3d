using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [SerializeField] private Grid _grid;
    [SerializeField] private GameObject _gridLines;
    [SerializeField] private LineRenderer _pressOutline;
    
    [SerializeField] private MeshRenderer _selectionSquare;
    private List<MeshRenderer> _selectionSquares;

    [Header("Test Visuals")]
    //[SerializeField] private GameObjectList _testTileTerrainVisuals;
    
    [Header("Test Map Parameters")]
    [SerializeField] private int _testMapX = 8;
    [SerializeField] private int _testMapY = 8;
    [SerializeField] private int _testMapPlayerCount = 2;
    [SerializeField] private int _testMapUnitCostTotal = 10;
    [SerializeField] private TileSelector _testTileSelector;
    
    [Header("Test Team Parameters")]
    [SerializeField] private List<int> _testTeamStartPositions;
    [SerializeField] private List<int> _testTeamUnitIds;
    
    [SerializeField] private Grid2D _grid2D;
    
    private Camera _mainCamera;
    private InputAction _selectAction;
    
    void Start()
    {
        _mainCamera = Camera.main;
        _selectAction = InputSystem.actions.FindAction("Player/Select");
        _grid.gameObject.SetActive(true);
        LoadMapData();
        InitSelectionOutlines();
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
        EngineUtil.GetMouseWorldPosition(_mainCamera, out var mouseWorldPosition, out var error);
        if (error)
        {
            _pressOutline.gameObject.SetActive(false);
            return;
        }
        Vector3Int gridPosition = _grid.WorldToCell(mouseWorldPosition);
        if (gridPosition.x >= 0 && gridPosition.x < _grid2D.X && gridPosition.y >= 0 && gridPosition.y < _grid2D.Y)
        {
            ShowTiles(
                _testTileSelector.GetTileSet(_grid2D, (gridPosition.x, gridPosition.y))
                );
            _pressOutline.gameObject.SetActive(true);
            _pressOutline.transform.position = _grid.CellToWorld(gridPosition);
        }
        else
        {
            _pressOutline.gameObject.SetActive(false);
        }
    }
    
    //
    // TESTING
    //
    
    private void LoadMapData(MapData mapData=null)
    {
        mapData ??= CreateTestMapData(_testMapX, _testMapY, _testMapPlayerCount, _testMapUnitCostTotal);
        _grid2D = new Grid2D(mapData);
        RenderGrid();
    }
    
    private void LoadTeamData(TeamData teamData = null)
    {
        teamData ??= CreateTestTeamData(_testTeamStartPositions, _testTeamUnitIds);
        
    }
    
    private static MapData CreateTestMapData(int testMapX = 8, int testMapY = 8, int testMapPlayerCount = 2, int testUnitCostTotal = 16)
    {
        var testEntityStartPositions = new List<int>();
        for (var i = 0; i < testMapX; i++)
        {
            if (i == 0) testEntityStartPositions.AddRange(Enumerable.Repeat(-1, testMapY).ToList());
            else if (i == testMapX - 1) testEntityStartPositions.AddRange(Enumerable.Repeat(-2, testMapY).ToList());
            else testEntityStartPositions.AddRange(Enumerable.Repeat(0, testMapY).ToList());
        }
        var testTileTerrain = Enumerable.Repeat(TileTerrain.Default, testMapX * testMapY).ToList();
        const int testMapId = 0;
        const string testMapName = "Test Map";
        return new MapData(testMapId, testMapName, testMapX, testMapY, testMapPlayerCount, testUnitCostTotal, testEntityStartPositions, testTileTerrain);
    }
    
    private static TeamData CreateTestTeamData(List<int> testTeamStartPositions=null, List<int> testTeamUnitIds=null)
    {
        const int testMapId = 0;
        const string testMapName = "Test Map";
        return new TeamData(testMapName, testMapId, testTeamStartPositions, testTeamUnitIds);
    }
    
    //
    // RENDERING
    //
    
    private void InitSelectionOutlines()
    {
        _selectionSquares = new List<MeshRenderer>();
        for (var x = 0; x < _grid2D.X; x++)
        {
            for (var y = 0; y < _grid2D.Y; y++)
            {
                _selectionSquares.Add(Instantiate(_selectionSquare, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, -0.1f, 0.5f),  Quaternion.identity));
            }
        }
    }
    
    private void ShowTiles(HashSet<int> tiles)
    {
        for (int i = 0; i < _selectionSquares.Count; i++) { _selectionSquares[i].gameObject.SetActive(false); }
        foreach (var tile in tiles)
        {
            if (_grid2D.IsValidPosition(tile))
            {
                _selectionSquares[tile].gameObject.SetActive(true);
                Debug.Log(_grid2D.ToPosition2D(tile));
            }
        }
    }
    
    private void RenderGrid()
    {
        _gridLines.SetActive(true);
        _gridLines.transform.position = new Vector3(_grid2D.X/2.0f, -0.1f, _grid2D.Y/2.0f);
        _gridLines.transform.localScale = new Vector3(_grid2D.X/10f, 1, _grid2D.Y/10f);
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
