using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig SummonBat = new()
    {
        Id = "Summon Bat",
        DisplayName = "Summon Bat",
        Description = "Summon a Bat on an adjacent square.",
        Cooldown = 3,
        Warmup = 3,
        Effects = new List<AbilityEffect>
        {
            new SpawnEffect { Unit = "Bat" }
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
