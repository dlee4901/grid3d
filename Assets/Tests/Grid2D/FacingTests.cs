using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class FacingTests
{
    private static bool Cast(TurnExecutor executor, int player, string ability, int source, int target)
        => executor.Apply(new AbilityCommand(player, ability, source, new[] { target }));

    private static void Pass(TurnExecutor executor, int player) => Assert.IsTrue(executor.Apply(new PassCommand(player)));

    private static Entity At(TurnExecutor executor, int position) => executor.State.GetEntity(position);

    private static int Health(TurnExecutor executor, int position)
    {
        At(executor, position).TryGetComponent<HealthComponent>(out var health);
        return health.Current;
    }

    private static void Face(TurnExecutor executor, int position, GridDirectionFacing facing)
        => typeof(Entity).GetProperty(nameof(Entity.Facing)).SetValue(At(executor, position), facing);

    private static string Kit(string kit) => TestContent.UnitWith(kit);

    private static TurnExecutor StartUnits(int x, int y, (int Position, string Unit)[] player1, (int Position, string Unit)[] player2)
    {
        var map = new GridDefinition
        {
            Id = $"Test.Map.Facing.{x}x{y}", X = x, Y = y, MaxTeamCost = 100,
            PlayerStartPositions = new List<PositionConfig>
            {
                new() { PositionValues = player1.Select(unit => unit.Position).ToList() },
                new() { PositionValues = player2.Select(unit => unit.Position).ToList() }
            }
        };
        map.Init();
        var executor = TurnExecutor.ForDefinition(map);
        executor.Apply(new LoadTeamCommand(1, new TeamData("Test.Team", map.Id, new List<(int, string)>(player1))));
        executor.Apply(new LoadTeamCommand(2, new TeamData("Test.Team", map.Id, new List<(int, string)>(player2))));
        executor.Apply(new SpawnTeamsCommand());
        return executor;
    }

    [TestCase(GridDirectionFacing.East, GridDirection.North, GridDirection.East)]
    [TestCase(GridDirectionFacing.East, GridDirection.NorthEast, GridDirection.SouthEast)]
    [TestCase(GridDirectionFacing.East, GridDirection.NorthCone, GridDirection.EastCone)]
    [TestCase(GridDirectionFacing.East, GridDirection.Vertical, GridDirection.Horizontal)]
    [TestCase(GridDirectionFacing.East, GridDirection.Orthogonal, GridDirection.Orthogonal)]
    [TestCase(GridDirectionFacing.West, GridDirection.North, GridDirection.West)]
    [TestCase(GridDirectionFacing.West, GridDirection.NorthEast, GridDirection.NorthWest)]
    [TestCase(GridDirectionFacing.West, GridDirection.NorthCone, GridDirection.WestCone)]
    [TestCase(GridDirectionFacing.West, GridDirection.Horizontal, GridDirection.Vertical)]
    [TestCase(GridDirectionFacing.West, GridDirection.SouthCone, GridDirection.EastCone)]
    public void Rotate_TurnsRelativeDirection(GridDirectionFacing facing, GridDirection relative, GridDirection expected)
    {
        Assert.AreEqual(expected, GridTraversal.Rotate(relative, facing));
    }

    [TestCase(GridDirectionFacing.East, new[] { 2, 5, 8 })]
    [TestCase(GridDirectionFacing.West, new[] { 0, 3, 6 })]
    public void ForwardCone_FollowsFacing(GridDirectionFacing facing, int[] expected)
    {
        var executor = TestGame.StartOn(3, 3, new[] { (4, TestContent.Tough) }, new[] { (1, TestContent.Tough) });
        Face(executor, 4, facing);
        var cone = new GridTraversal { Direction = GridDirection.NorthCone, Linear = false, MaxDistance = 1, Passthrough = Occupant.Any };
        var positions = cone.GetSteps(new GridSource(executor.State, At(executor, 4).Position, At(executor, 4)))
            .Where(step => step.Distance > 0).Select(step => step.Position.Dim1).OrderBy(p => p);
        CollectionAssert.AreEqual(expected, positions);
    }

    [Test]
    public void Turret_FacingEast_ShootsEast()
    {
        var executor = StartUnits(6, 2, new[] { (0, "Turret"), (6, Kit(TestContent.Tough)) }, new[] { (3, Kit(TestContent.Tough)), (11, Kit(TestContent.Tough)) });
        Face(executor, 0, GridDirectionFacing.East);
        Pass(executor, 1);
        Assert.AreEqual(7, Health(executor, 3));
    }

    [Test]
    public void Turret_FacingWest_ShootsWest()
    {
        var executor = StartUnits(6, 2, new[] { (5, "Turret"), (11, Kit(TestContent.Tough)) }, new[] { (2, Kit(TestContent.Tough)), (6, Kit(TestContent.Tough)) });
        Face(executor, 5, GridDirectionFacing.West);
        Pass(executor, 1);
        Assert.AreEqual(7, Health(executor, 2));
    }

    [TestCase(GridDirectionFacing.East)]
    [TestCase(GridDirectionFacing.West)]
    public void SwordSlash_AimedNorth_HitsNorthRow(GridDirectionFacing facing)
    {
        var executor = TestGame.StartOn(3, 3,
            new[] { (4, TestContent.SwordSlash) },
            new[] { (6, TestContent.Tough), (7, TestContent.Tough), (8, TestContent.Tough), (2, TestContent.Tough) });
        Face(executor, 4, facing);
        Assert.IsTrue(Cast(executor, 1, TestContent.SwordSlash, 4, 7));
        Assert.AreEqual(6, Health(executor, 6));
        Assert.AreEqual(6, Health(executor, 7));
        Assert.AreEqual(6, Health(executor, 8));
        Assert.AreEqual(10, Health(executor, 2));
    }

    [TestCase(GridDirectionFacing.East)]
    [TestCase(GridDirectionFacing.West)]
    public void SwordSlash_AimedEast_HitsEastColumn(GridDirectionFacing facing)
    {
        var executor = TestGame.StartOn(3, 3,
            new[] { (4, TestContent.SwordSlash) },
            new[] { (2, TestContent.Tough), (5, TestContent.Tough), (8, TestContent.Tough), (6, TestContent.Tough) });
        Face(executor, 4, facing);
        Assert.IsTrue(Cast(executor, 1, TestContent.SwordSlash, 4, 5));
        Assert.AreEqual(6, Health(executor, 2));
        Assert.AreEqual(6, Health(executor, 5));
        Assert.AreEqual(6, Health(executor, 8));
        Assert.AreEqual(10, Health(executor, 6));
    }

    [TestCase(GridDirectionFacing.East)]
    [TestCase(GridDirectionFacing.West)]
    public void Incinerate_AimedEast_ConeFallsOffEastward(GridDirectionFacing facing)
    {
        var executor = TestGame.StartOn(5, 5,
            new[] { (12, TestContent.IncinerateKit) },
            new[] { (13, TestContent.Tough), (19, TestContent.Tough), (11, TestContent.Tough) });
        Face(executor, 12, facing);
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.Incinerate.Id, 12, 13));
        Assert.AreEqual(6, Health(executor, 13));
        Assert.AreEqual(7, Health(executor, 19));
        Assert.AreEqual(10, Health(executor, 11));
    }

    [Test]
    public void AbsoluteDirection_IgnoresFacing()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (4, TestContent.Tough) }, new[] { (1, TestContent.Tough) });
        Face(executor, 4, GridDirectionFacing.East);
        var ray = new GridTraversal { Direction = GridDirection.North, MaxDistance = 1, AbsoluteDirection = true, Passthrough = Occupant.Any };
        var positions = ray.GetSteps(new GridSource(executor.State, At(executor, 4).Position, At(executor, 4)))
            .Where(step => step.Distance > 0).Select(step => step.Position.Dim1);
        CollectionAssert.AreEqual(new[] { 7 }, positions);
    }

    [Test]
    public void TriggeredAbilityDamage_CountsAsAbilityDamage()
    {
        var executor = StartUnits(2, 6, new[] { (0, "Turret"), (1, Kit(TestContent.Tough)) }, new[] { (4, Kit(TestContent.Armored)) });
        Pass(executor, 1);
        Assert.AreEqual(8, Health(executor, 4));
    }
}
