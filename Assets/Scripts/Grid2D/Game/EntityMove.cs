public readonly struct EntityMove
{
    public int Uid { get; }
    public GridPosition From { get; }
    public GridPosition To { get; }
    public bool Teleport { get; }

    public EntityMove(int uid, GridPosition from, GridPosition to, bool teleport = false)
    {
        Uid = uid;
        From = from;
        To = to;
        Teleport = teleport;
    }
}
