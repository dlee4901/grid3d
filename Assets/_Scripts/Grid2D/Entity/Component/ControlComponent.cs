public sealed class ControlComponent
{
    public int PlayerId { get; private set; }
    
    private readonly Entity _entity;
    
    public ControlComponent(Entity entity, int playerId=0)
    {
        _entity = entity;
        PlayerId = playerId;
    }
    
    public void SetPlayerId(int playerId)
    {
        PlayerId = playerId;
    }
    
    public bool HasSameController(Entity entity) => entity.Control != null && entity.Control.PlayerId == PlayerId;
}
