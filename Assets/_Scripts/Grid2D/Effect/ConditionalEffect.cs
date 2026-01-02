public sealed class ConditionalEffect : Effect
{
    private readonly ICondition _condition;
    private readonly Effect _effect;
}