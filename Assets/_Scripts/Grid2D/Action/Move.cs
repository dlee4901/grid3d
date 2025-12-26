using System;
using System.Collections.Generic;

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
    
    public HashSet<int> GetSelectableTiles(Grid2D grid, int position)
    {
        HashSet<int> moves = new();
        foreach (var tileSelector in TileSelectors)
        {
            moves.UnionWith(tileSelector.GetTileSet(grid, position));
        }
        moves.Remove(position);
        moves.ExceptWith(grid.GetOccupiedTilesPositionSet());
        return moves;
    }
}