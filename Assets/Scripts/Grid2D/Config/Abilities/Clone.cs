using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Clone = new()
    {
        Id = "Clone",
        DisplayName = "Clone",
        Description = "Create an exact copy of this unit on an adjacent square.",
        Cooldown = 2,
        Effects = new List<AbilityEffect>
        {
            new SpawnEffect { Exact = true }
        },
        Targeting = new PositionTargeting
        {
            SelectableArea = new GridSelection
            {
                MinDistance = 1,
                Traversals = new List<GridTraversal>
                {
                    new GridTraversal { Direction = GridDirection.Orthogonal, Linear = false, MaxDistance = 1 }
                }
            }
        }
    };
}
