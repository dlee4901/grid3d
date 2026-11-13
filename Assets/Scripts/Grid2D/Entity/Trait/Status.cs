public sealed class Status : Trait
{
    private int _count;
    public override int Count => _count;

    internal Status(StatusConfig config, int count) : base(config) => _count = count;

    internal void Adjust(int amount) => _count += amount;
}
