using NUnit.Framework;

public class TracedPathTests
{
    private static string Move => AbilityConfigs.MoveStraight3Step.Id;

    private static TurnExecutor Start()
        => TestGame.StartOn(5, 3, new[] { (0, TestContent.GroundedKit) }, new[] { (14, TestContent.Tough) });

    private static bool Trace(TurnExecutor executor, params int[] path)
        => executor.Apply(new AbilityCommand(1, Move, 0, new[] { path[path.Length - 1] }, path));

    [Test]
    public void TracedPath_IsFollowedInsteadOfShortest()
    {
        var executor = Start();
        Assert.IsTrue(Trace(executor, 5, 6, 1));
        var moves = executor.State.Moves;
        Assert.AreEqual(3, moves.Count);
        Assert.AreEqual(5, moves[0].To.Dim1);
        Assert.AreEqual(6, moves[1].To.Dim1);
        Assert.AreEqual(1, moves[2].To.Dim1);
    }

    [Test]
    public void TracedPath_RejectsRevisit()
    {
        var executor = Start();
        Assert.IsFalse(Trace(executor, 1, 2, 1));
        Assert.IsNotNull(executor.State.GetEntity(0));
    }

    [Test]
    public void TracedPath_RejectsReturningToStart()
    {
        Assert.IsFalse(Trace(Start(), 1, 0));
    }

    [Test]
    public void TracedPath_RejectsLongerThanMoveRange()
    {
        Assert.IsFalse(Trace(Start(), 5, 6, 7, 2));
    }

    [Test]
    public void TracedPath_RejectsNonAdjacentStep()
    {
        Assert.IsFalse(Trace(Start(), 6));
    }

    [Test]
    public void TracedPath_MustEndOnTarget()
    {
        var executor = Start();
        Assert.IsFalse(executor.Apply(new AbilityCommand(1, Move, 0, new[] { 2 }, new[] { 1 })));
    }
}
