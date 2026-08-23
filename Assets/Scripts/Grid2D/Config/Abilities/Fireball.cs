using System.Collections.Generic;
using UnityEngine;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Fireball = new()
    {
        Id = "Fireball",
        ManaCost = 2,
        Effects = new List<AbilityEffect>()
        {
            new DamageEffect()
            {
                Relation = EntityRelation.Any & ~EntityRelation.Self,
                Amount = 3
            }
        },
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
                        Passthrough = EntityRelation.Any
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
                        Passthrough = EntityRelation.Any
                    }
                }
            }
        }
    };
}
