public class ControlComponent : IEntityComponent
{
    public int PlayerController { get; set; } = 0;
    
    public ControlComponent(int playerController)
    {
        PlayerController = playerController;
    }
    
    public bool IsAlly(ControlComponent controlComponent)
        => controlComponent.PlayerController == PlayerController;
        
    public bool IsAlly(int playerController)
        => playerController == PlayerController;
}