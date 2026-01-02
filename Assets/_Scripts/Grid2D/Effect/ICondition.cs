public interface ICondition
{
    bool Evaluate<T>(QueryBuilder<T> query);
}