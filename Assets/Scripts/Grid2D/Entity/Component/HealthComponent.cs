public class HealthComponent : IEntityComponent
{
    public int Starting { get; set; } = 0;
    public int Current { get; set; } = 0;
    
    public HealthComponent(int health)
    {
        Starting = health;
        Current = health;
    }
    
    static HealthComponent()
    {
        //AccessorRegistry<Entity>.Register<int>("StartingHealth", e => e.Health?.Starting ?? 0);
    }
}