using System;
using System.Collections.Generic;

public sealed class Move : IComponent
{
    public TileSelectionBuilder TileSelectionBuilder { get; }
    
    public Move(TileSelectionBuilder tileSelectionBuilder)
    {
        TileSelectionBuilder = tileSelectionBuilder;
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
    
    public HashSet<int> GetSelectableTiles()
    {
        var query = QueryBuilder<Entity>.Match(e => e.GetType == typeof(Entity));
        var tileDistances = TileSelectionBuilder.GetTileDistances();
        
    }
}