public struct PlayerCommand
{
    public int PlayerId;
    public int Turn;
    
    public int Action; // 0 = move, 1~n = ability
    
    public int SourceTile;
    public int TargetCount;
    public int[] TargetTiles;
}