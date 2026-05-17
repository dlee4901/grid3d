public interface IReadOnlyEntity
{
    string Id { get; }
    int Cost { get; }
    int Position { get; }
    DirectionFacing Facing { get; }
    bool TryGetComponent<T>(out T component) where T : class, IEntityComponent;
}
