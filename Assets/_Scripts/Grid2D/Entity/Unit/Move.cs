using System;
using System.Collections.Generic;

[Serializable]
public class Move
{
    public List<GridSelection> TileSelections { get; private set; }
    
    public Move(List<GridSelection> tileSelections)
    {
        TileSelections = tileSelections;
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
        foreach (GridSelection tileSelection in TileSelections)
        {
            moves.UnionWith(tileSelection.GetTiles(grid, grid.ToPosition2D(position1D)));
        }
        moves.Remove(position1D);
        moves.ExceptWith(grid.GetOccupiedTilesPosition1DSet());
        return moves;
    }
}