using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig SummonSkeleton = new()
    {
        Id = "Summon Skeleton",
        DisplayName = "Summon Skeleton",
        Description = "Summon a Skeleton with Revive on an adjacent square.",
        Cooldown = 2,
        Effects = new List<AbilityEffect>
        {
            new SpawnEffect
            {
                Unit = "Skeleton",
                Statuses = { ("Revive", 1) }
            }
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
