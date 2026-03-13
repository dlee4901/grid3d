public class Health : IEntityComponent
{
    public int Starting { get; set; } = 0;
    public int Current { get; set; } = 0;
    
    static Health()
    {
        //AccessorRegistry<Entity>.Register<int>("StartingHealth", e => e.Health?.Starting ?? 0);
    }
}