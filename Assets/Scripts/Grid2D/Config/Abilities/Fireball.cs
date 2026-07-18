using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Fireball = new()
    {
        Id = "Fireball",
        Selection = new AreaAbilitySelection()
        {
            EffectArea = new GridSelection()
            {
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = DirectionType.Straight,
                        Linear = false,
                        MaxDistance = 3
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
                            MaxDistance = 3
                        }
                    }
                }
            }
        }
    };
}
