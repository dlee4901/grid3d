using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig FlameBurst = new()
    {
        Id = "Flame Burst",
        Targeting = new FillTargeting()
        {
            EffectArea = new GridSelection()
            {
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = GridDirection.Line,
                        MaxDistance = 2,
                        Passthrough = EntityRelation.Any
                    }
                }
            }
        }
    };
}