using System.Collections.Generic;

public interface ITriggered
{
    string Id { get; }
    Trigger Trigger { get; }
    GridSelection Area { get; }
    List<AbilityEffect> Effects { get; }
    int Count { get; }
}
