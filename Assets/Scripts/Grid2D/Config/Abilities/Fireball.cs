using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Fireball = new()
    {
        Id = "Fireball",
        Targeting = new AreaAbilityTargeting()
        {
            EffectArea = new GridSelection()
            {
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = DirectionType.Straight,
                        Linear = false,
                        MaxDistance = 3,
                        Passthrough = EntityPassthrough.All
                    }
                }
            },
            SelectableAreas = new List<GridSelection>()
            {
                new GridSelection()
                {
                    Traversals = new List<GridTraversal>()
                    {
                        new GridTraversal()
                        {
                            Direction = DirectionType.Straight,
                            Linear = false,
                            MaxDistance = 3,
                            Passthrough = EntityPassthrough.All
                        }
                    }
                }
            }
        }
    };
}
