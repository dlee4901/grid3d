public sealed class HealthComponent
{
    public int Starting { get; }
    public int Current { get; private set; }
    
    static HealthComponent()
    {
        AccessorRegistry<Entity>.Register<int>("StartingHealth", e => e.Health?.Starting ?? 0);
    }
    
    public HealthComponent(int starting)
    {
        Starting = starting;
        Current = starting;
    }
    
    public void SetCurrent(int current)
    {
        Current = current;
    }
}