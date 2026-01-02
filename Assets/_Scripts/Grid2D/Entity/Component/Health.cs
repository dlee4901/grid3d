public sealed class Health : IComponent
{
    public int Starting { get; }
    public int Current { get; }
    
    public Health(int starting)
    {
        Starting = starting;
        Current = starting;
    }
}