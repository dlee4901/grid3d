using System;
using System.Collections.Generic;

[Serializable]
public class Move
{
    public List<TileSelector> TileSelectors { get; private set; }
    
    public Move(List<TileSelector> tileSelectors)
    {
        TileSelectors = tileSelectors;
    }

    // public Move(MoveStruct moveStruct)
    // {
    //     GridSelectionStruct[] gridSelectionStructs = moveStruct.GridSelections;
    //     GridSelections = new List<GridSelection>();
    //     foreach (GridSelectionStruct gridSelectionStruct in gridSelectionStructs)
    //     {
    //         GridSelections.Add(new GridSelection(gridSelectionStruct));
    //     }
    // }
    
    public HashSet<int> GetSelectableTiles(Grid2D grid, int position1D)
    {
        HashSet<int> moves = new();
        foreach (var tileSelector in TileSelectors)
        {
            moves.UnionWith(tileSelector.GetTileSet(grid, grid.ToPosition2D(position1D)));
        }
        moves.Remove(position1D);
        moves.ExceptWith(grid.GetOccupiedTilesPositionSet());
        return moves;
    }
}