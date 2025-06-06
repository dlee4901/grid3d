using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [SerializeField] private LineRenderer _selectionOutline;
    [SerializeField] private GameObject _gridLines;
    [SerializeField] private Grid _grid;
    
    private Grid2D _grid2D;
    
    private Camera _mainCamera;
    private InputAction _selectAction;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainCamera = Camera.main;
        _selectAction = InputSystem.actions.FindAction("Player/Select");
        LoadMap();
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

    private void LoadMap(MapData mapData=null)
    {
        mapData ??= CreateTestData();
        _grid2D = new Grid2D(mapData);
        RenderGrid(_grid2D.X, _grid2D.Y);
    }
    
    private static MapData CreateTestData()
    {
        return new MapData
        {
            X = 10,
            Y = 10,
            UnitCostTotal = 20, 
            EntityStartPositions = new List<int>{-1, -1, -1, -1, -1, -1, -1, -1, -1, -1
                -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                -2, -2, -2, -2, -2, -2, -2, -2, -2, -2,
                -2, -2, -2, -2, -2, -2, -2, -2, -2, -2},
            TileTerrain = new List<TileTerrain>()
        };
        // return new MapData
        // {
        //     X = 8,
        //     Y = 8,
        //     UnitCostTotal = 16, 
        //     EntityStartPositions = new List<int>{-1, -1, -1, -1, -1, -1, -1, -1, 
        //         -1, -1, -1, -1, -1, -1, -1, -1,
        //         0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0,
        //         -2, -2, -2, -2, -2, -2, -2, -2,
        //         -2, -2, -2, -2, -2, -2, -2, -2},
        //     TileTerrain = new List<TileTerrain>()
        // };
    }

    private void RenderGrid(int x, int y)
    {
        Debug.Log(new Vector3(x, 0, y));
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
