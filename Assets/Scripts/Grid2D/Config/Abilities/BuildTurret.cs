using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig BuildTurret = new()
    {
        Id = "Build Turret",
        DisplayName = "Build Turret",
        Description = "Build a Turret on an adjacent square.",
        Cooldown = 3,
        Effects = new List<AbilityEffect>
        {
            new SpawnEffect { Unit = "Turret" }
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
