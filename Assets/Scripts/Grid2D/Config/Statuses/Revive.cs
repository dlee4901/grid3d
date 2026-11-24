using System.Collections.Generic;

public static partial class StatusConfigs
{
    public static readonly StatusConfig Revive = new()
    {
        Id = "Revive",
        DisplayName = "Revive",
        Description = "When this unit dies, a new copy of it is summoned where it died.",
        Counter = StatusCounter.None,
        Trigger = new Trigger { On = GameEvent.Died },
        Area = new GridSelection
        {
            Traversals = new List<GridTraversal>
            {
                new GridTraversal { Direction = GridDirection.Orthogonal, MaxDistance = 0 }
            }
        },
        Effects = new List<AbilityEffect>
        {
            new SpawnEffect()
        }
    };
}
