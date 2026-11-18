public readonly struct EntityMove
{
    public int Uid { get; }
    public GridPosition From { get; }
    public GridPosition To { get; }

    public EntityMove(int uid, GridPosition from, GridPosition to)
    {
        Uid = uid;
        From = from;
        To = to;
    }
}
