using System;

[Flags]
public enum EntityRelation
{
    None = 0,
    Self = 1 << 0,
    Enemy = 1 << 1,
    Ally = 1 << 2,
    Neutral = 1 << 3,
    Any = Self | Enemy | Ally | Neutral
}

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