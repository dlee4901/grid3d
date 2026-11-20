using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig FlameStep = new()
    {
        Id = "Flame Step",
        DisplayName = "Flame Step",
        Description = "Teleport to any empty Burning square.",
        Cooldown = 2,
        Effects = new List<AbilityEffect> { new MoveEffect { Teleport = true } },
        Targeting = new PositionTargeting
        {
            SelectableArea = new GridSelection
            {
                Traversals = new List<GridTraversal>
                {
                    new() { Direction = GridDirection.Straight, Linear = false, MaxDistance = GridTraversal.Unlimited, Passthrough = Occupant.Any }
                },
                Selectable = Occupant.Empty,
                Terrain = new[] { TerrainType.Burning }
            }
        }
    };
}
