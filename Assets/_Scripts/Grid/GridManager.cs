using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private GameObject _gridLines;
    [SerializeField] private Grid _grid;
    
    private Grid2D _grid2D;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 GetSize()
    {
        return new Vector2(_grid2D.X, _grid2D.Y);
    }

    public void LoadMap(MapData mapData=null)
    {
        mapData ??= CreateTestData();
        _grid2D = new Grid2D(mapData);
        RenderGrid(_grid2D.X, _grid2D.Y);
    }
    
    private static MapData CreateTestData()
    {
        return new MapData
        {
            X = 8,
            Y = 8,
            UnitCostTotal = 16, 
            EntityStartPositions = new List<int>{-1, -1, -1, -1, -1, -1, -1, -1, 
                -1, -1, -1, -1, -1, -1, -1, -1,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0,
                -2, -2, -2, -2, -2, -2, -2, -2,
                -2, -2, -2, -2, -2, -2, -2, -2},
            TileTerrain = new List<TileTerrain>()
        };
    }

    private void RenderGrid(int x, int y)
    {
        Debug.Log(new Vector3(x, 0, y));
        _gridLines.transform.position = new Vector3(x/2.0f, 0.01f, y/2.0f);
        _gridLines.transform.localScale = new Vector3(x/10.0f, 1, y/10.0f);
        _grid.transform.position = new Vector3(x/2.0f, 0, y/2.0f);
    }
    
    private void HandleMousePosition()
    
    
    
    {
        Vector3 mouseWorldPosition = EngineUtil.GetMouseWorldPosition();
        Vector3Int gridPosition = _grid.WorldToCell(mouseWorldPosition);
        // if (gridPosition.x >= 0 && gridPosition.x < Grid.X && gridPosition.y >= 0 && gridPosition.y < Grid.Y)
        // {
        //     _tileHoverIndicator.gameObject.SetActive(true);
        //     _tileHoverIndicator.transform.position = _grid.CellToWorld(gridPosition);
        // }
        // else
        // {
        //     _tileHoverIndicator.gameObject.SetActive(false);
        // }
    }
}
