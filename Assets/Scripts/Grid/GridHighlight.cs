using UnityEngine;

public class GridHighlight : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _highlightSquare;
    
    [SerializeField] private Color _actionableUnit = new Color(255f, 255f, 255f, 64f);
    [SerializeField] private Color _abilityRange = new Color(255f, 255f, 255f, 128f);
    [SerializeField] private Color _selectableTarget = new Color(255f, 255f, 255f, 128f);
    
    private SpriteRenderer[] _highlightSquares;
    
    public void Init(IReadOnlyGridState gridState)
    {
        _highlightSquares = new SpriteRenderer[gridState.Size];
        for (var x = 0; x < gridState.X; x++)
        {
            for (var y = 0; y < gridState.Y; y++)
            {
                //_highlightSquares[gridState.ToPosition1D(x, y)] = Instantiate(_highlightSquare, _grid.CellToWorld(new Vector3Int(x, y, 0)) + new Vector3(0.5f, _gridGroundLevel + 0.01f, 0.5f), Quaternion.Euler(90f, 0f, 0f), gameObject.transform);
            }
        }
    }
}