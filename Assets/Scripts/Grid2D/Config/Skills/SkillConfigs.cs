using System.Collections.Generic;

public static class SkillConfigs
{
    public static readonly SkillConfig MoveStraight3Step = new()
    {
        Id = "MoveStraight3Step",
        Type = "Move",
        Selection = new SingleSkillSelection()
        {
            SelectableAreas = new List<GridSelection>()
            {
                new GridSelection()
                {
                    MinDistance = 1,
                    Traversals = new List<GridTraversal>()
                    {
                        new GridTraversal()
                        {
                            Direction = DirectionType.Straight,
                            MaxDistance = 3,
                            Linear = false
                        }
                    }
                }
            }
        }
    };
    
    public static readonly SkillConfig IcicleBlast = new()
    {
        Id = "IcicleBlast",
        Selection = new FillSkillSelection()
        {
            SelectableAreas = new List<GridSelection>()
            {
                new GridSelection()
                {
                    Traversals = new List<GridTraversal>()
                    {
                        new GridTraversal()
                        {
                            Direction = DirectionType.North,
                            MaxDistance = 6
                        }
                    }
                },
                new GridSelection()
                {
                    Traversals = new List<GridTraversal>()
                    {
                        new GridTraversal()
                        {
                            Direction = DirectionType.South,
                            MaxDistance = 6
                        }
                    }
                }
            }
        }
    };
}