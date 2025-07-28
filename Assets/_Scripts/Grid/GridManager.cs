using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [SerializeField] private LineRenderer _selectionOutline;
    [SerializeField] private GameObject _gridLines;
    [SerializeField] private Grid _grid;

    [Header("Test Visuals")]
    [SerializeField] private GameObjectList _testTileTerrainVisuals;
    
    [Header("Test Map Parameters")]
    [SerializeField] private string _testMapName;
    [SerializeField] private int _testMapX;
    [SerializeField] private int _testMapY;
    [SerializeField] private int _testMapPlayerCount;
    [SerializeField] private int _testMapUnitCostTotal;
    
    [Header("Test Team Parameters")]
    [SerializeField] private string _testTeamName;
    [SerializeField] private List<int> _testTeamStartPositions;
    [SerializeField] private List<int> _testTeamUnitIds;
    
    private Grid2D _grid2D;
    
    private Camera _mainCamera;
    private InputAction _selectAction;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainCamera = Camera.main;
        _selectAction = InputSystem.actions.FindAction("Player/Select");
        _grid.gameObject.SetActive(true);
        LoadMapData();
    }

    // Update is called once per frame
    void Update()
    {
        if (_selectAction.triggered) HandleMousePosition();
    }

    public Vector2 GetSize()
    {
        return new Vector2(_grid2D.X, _grid2D.Y);
    }
    
    private void LoadMapData(MapData mapData=null)
    {
        mapData ??= CreateTestMapData(_testMapName, _testMapX, _testMapY, _testMapPlayerCount, _testMapUnitCostTotal);
        _grid2D = new Grid2D(mapData);
        RenderGrid(_grid2D.X, _grid2D.Y);
    }
    
    private void LoadTeamData(TeamData teamData = null)
    {
        teamData ??= CreateTestTeamData(_testTeamName, _testMapName, _testTeamStartPositions, _testTeamUnitIds);
        
    }
    
    private static MapData CreateTestMapData(string testMapName="Test Map", int testMapX = 8, int testMapY = 8, int testMapPlayerCount = 2, int testUnitCostTotal = 16)
    {
        var testEntityStartPositions = new List<int>();
        for (var i = 0; i < testMapX; i++)
        {
            if (i == 0) testEntityStartPositions.AddRange(Enumerable.Repeat(-1, testMapY).ToList());
            else if (i == testMapX - 1) testEntityStartPositions.AddRange(Enumerable.Repeat(-2, testMapY).ToList());
            else testEntityStartPositions.AddRange(Enumerable.Repeat(0, testMapY).ToList());
        }
        var testTileTerrain = Enumerable.Repeat(TileTerrain.Default, testMapX * testMapY).ToList();
        return new MapData(testMapName, testMapX, testMapY, testMapPlayerCount, testUnitCostTotal, testEntityStartPositions, testTileTerrain);
    }
    
    private static TeamData CreateTestTeamData(string testTeamName="Test Team", string testMapName="Test Map", List<int> testTeamStartPositions=null, List<int> testTeamUnitIds=null)
    {
        return new TeamData(testTeamName, testMapName, testTeamStartPositions, testTeamUnitIds);
    }

    private void RenderGrid(int x, int y)
    {
        _gridLines.SetActive(true);
        _gridLines.transform.position = new Vector3(x/2.0f, 0.01f, y/2.0f);
        _gridLines.transform.localScale = new Vector3(x/10.0f, 1, y/10.0f);
    }
    
    private void HandleMousePosition()
    {
        EngineUtil.GetMouseWorldPosition(_mainCamera, out var mouseWorldPosition, out var error);
        if (error)
        {
            _selectionOutline.gameObject.SetActive(false);
            return;
        }
        Vector3Int gridPosition = _grid.WorldToCell(mouseWorldPosition);
        if (gridPosition.x >= 0 && gridPosition.x < _grid2D.X && gridPosition.y >= 0 && gridPosition.y < _grid2D.Y)
        {
            _selectionOutline.gameObject.SetActive(true);
            _selectionOutline.transform.position = _grid.CellToWorld(gridPosition);
        }
        else
        {
            _selectionOutline.gameObject.SetActive(false);
        }
    }
}
