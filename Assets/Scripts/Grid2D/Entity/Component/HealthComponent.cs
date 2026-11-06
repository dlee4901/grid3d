public class HealthComponent : IEntityComponent
{
    public int Starting { get; internal set; } = 0;
    public int Current { get; internal set; } = 0;
    
    public HealthComponent(int health)
    {
        Starting = health;
        Current = health;
    }
    
    internal bool ApplyDamage(int amount)
    {
        if (amount <= 0) return false;
        Current = System.Math.Max(0, Current - amount);
        return Current == 0;
    }

    internal void ApplyHeal(int amount)
    {
        if (amount <= 0) return;
        Current = System.Math.Min(Starting, Current + amount);
    }

    
    static HealthComponent()
    {
        //AccessorRegistry<Entity>.Register<int>("StartingHealth", e => e.Health?.Starting ?? 0);
    }
}