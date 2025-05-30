using System;
using System.Collections.Generic;

[Serializable]
public class Move
{
    public List<GridSelection> GridSelections { get; private set; }
    
    public Move(List<GridSelection> gridSelections)
    {
        GridSelections = gridSelections;
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
        foreach (GridSelection gridSelection in GridSelections)
        {
            moves.UnionWith(gridSelection.GetSelectableTiles(grid, grid.ToPosition2D(position1D)));
        }
        moves.Remove(position1D);
        moves.ExceptWith(grid.GetOccupiedTilesPosition1DSet());
        return moves;
    }
}