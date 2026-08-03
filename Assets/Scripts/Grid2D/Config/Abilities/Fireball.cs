using System.Collections.Generic;
using UnityEngine;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Fireball = new()
    {
        Id = "Fireball",
        Targeting = new PositionTargeting()
        {
            EffectArea = new GridSelection()
            {
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = GridDirection.Straight,
                        Linear = false,
                        MaxDistance = 2,
                        Passthrough = EntityPassthrough.All
                    }
                }
            },
            SelectableArea = new GridSelection()
            {
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = GridDirection.Straight,
                        Linear = false,
                        MaxDistance = 2,
                        Passthrough = EntityPassthrough.All
                    }
                }
            }
        }
    };
}
