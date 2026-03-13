public class Control : IEntityComponent
{
    public int PlayerId { get; set; } = 0;
    
    public bool IsAlly(Control control)
        => control.PlayerId == PlayerId;
        
    public bool IsAlly(int playerId)
        => playerId == PlayerId;
}