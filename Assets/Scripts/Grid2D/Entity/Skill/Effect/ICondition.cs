public interface ICondition
{
    bool Evaluate<T>(PredicateBuilder<T> predicate);
}