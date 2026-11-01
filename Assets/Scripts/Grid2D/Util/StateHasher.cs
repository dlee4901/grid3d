public struct StateHasher
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    private ulong _value;

    public static StateHasher Create() => new() { _value = OffsetBasis };

    public ulong Value => _value;

    public void Add(int value)
    {
        unchecked
        {
            for (var shift = 0; shift < 32; shift += 8)
            {
                _value ^= (byte)(value >> shift);
                _value *= Prime;
            }
        }
    }

    public void Add(bool value) => Add(value ? 1 : 0);

    public void Add(string value)
    {
        if (value == null)
        {
            Add(-1);
            return;
        }
        Add(value.Length);
        foreach (var c in value) Add(c);
    }
}
