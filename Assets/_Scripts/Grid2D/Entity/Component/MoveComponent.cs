public sealed class MoveComponent
{
    public TileSelectionBuilder TileSelectionBuilder { get; }
    
    private readonly Entity _entity;
    
    public MoveComponent(Entity entity, TileSelectionBuilder tileSelectionBuilder)
    {
        _entity = entity;
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
    
    // public HashSet<int> GetSelectableTiles()
    // {
    //     var query = QueryBuilder<Entity>.Match(e => e.GetType == typeof(Entity));
    //     var tileDistances = TileSelectionBuilder.GetTileDistances();
    //     
    // }
}