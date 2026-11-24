using System.Collections.Generic;

public sealed class Reaction
{
    public Trigger Trigger { get; init; }
    public GridSelection Area { get; init; }
    public List<AbilityEffect> Effects { get; init; } = new();
}

public sealed class ReactionInstance : ITriggered
{
    public ITriggered Owner { get; }
    private readonly Reaction _reaction;

    public string Id => Owner.Id;
    public int Count => Owner.Count;
    public Trigger Trigger => _reaction.Trigger;
    public GridSelection Area => _reaction.Area;
    public List<AbilityEffect> Effects => _reaction.Effects;

    public static ITriggered OwnerOf(ITriggered triggered)
    {
        if (triggered is ReactionInstance reaction) return reaction.Owner;
        return triggered;
    }

    internal ReactionInstance(ITriggered owner, Reaction reaction)
    {
        Owner = owner;
        _reaction = reaction;
    }
}
