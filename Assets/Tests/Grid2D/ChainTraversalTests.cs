using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

public class ChainTraversalTests
{
    private const int Size = 7;

    private static GridState Board(int blocker)
    {
        var executor = TestGame.StartOn(Size, Size, new[] { (0, TestContent.Cheap) }, new[] { (blocker, TestContent.Cheap) });
        return executor.State;
    }

    private static int Index(int x, int y) => y * Size + x;

    private static GridTraversal Lance() => new()
    {
        Direction = GridDirection.Vertical, Linear = true, MaxDistance = 2, Passthrough = Occupant.Empty,
        Chain = new GridTraversal { Direction = GridDirection.All, Linear = false, MaxDistance = 1, Passthrough = Occupant.Any }
    };

    private static Dictionary<(int, int), GridStep> StepsByCell(GridState state)
    {
        var source = new GridSource(state, new GridPosition(state, (3, 3)));
        return Lance().GetStepsSet(source)
                      .GroupBy(step => step.Position.Dim2)
                      .ToDictionary(group => group.Key, group => group.OrderBy(step => step.Distance).First());
    }

    [Test]
    public void Chain_StartsAtRayEnds_WithAccumulatedDistance()
    {
        var steps = StepsByCell(Board(Index(3, 5)));

        Assert.AreEqual(1, steps[(3, 4)].Distance);
        Assert.AreEqual(2, steps[(2, 5)].Distance);
        Assert.AreEqual(GridDirection.North, steps[(2, 5)].Direction);

        Assert.AreEqual(2, steps[(3, 1)].Distance);
        Assert.AreEqual(3, steps[(2, 0)].Distance);
        Assert.AreEqual(3, steps[(4, 0)].Distance);
        Assert.AreEqual(GridDirection.South, steps[(2, 0)].Direction);
    }

    [Test]
    public void Chain_HasLocalMaxDistance_SameSizeOnEveryRay()
    {
        var steps = StepsByCell(Board(Index(3, 5)));

        Assert.IsTrue(steps.ContainsKey((3, 5)));
        Assert.IsFalse(steps.ContainsKey((3, 6)));
        Assert.IsTrue(steps.ContainsKey((3, 0)));
        Assert.IsFalse(steps.ContainsKey((1, 0)));
        Assert.IsFalse(steps.ContainsKey((5, 2)));
    }

    [TestCase(false, 3)]
    [TestCase(true, 2)]
    public void ChainAll_StartsChainAtEveryStep(bool chainAll, int expectedDistance)
    {
        var state = Board(Index(0, 6));
        var source = new GridSource(state, new GridPosition(state, (3, 3)));
        var traversal = new GridTraversal
        {
            Direction = GridDirection.North, Linear = true, MaxDistance = 2, Passthrough = Occupant.Empty, ChainAll = chainAll,
            Chain = new GridTraversal { Direction = GridDirection.All, Linear = false, MaxDistance = 1, Passthrough = Occupant.Any }
        };
        var steps = traversal.GetStepsSet(source).Where(step => step.Position.Dim2 == (2, 4)).ToList();

        Assert.AreEqual(1, steps.Count);
        Assert.AreEqual(expectedDistance, steps[0].Distance);
    }

    [Test]
    public void Chain_NeverAddsCasterSquare()
    {
        var state = Board(Index(3, 5));
        var source = new GridSource(state, new GridPosition(state, (3, 3)));
        var atCaster = Lance().GetStepsSet(source).Where(step => step.Position.Dim2 == (3, 3)).ToList();

        Assert.IsTrue(atCaster.All(step => step.Distance == 0));
    }
}
