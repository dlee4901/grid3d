using NUnit.Framework;

public class MoveTrailTests
{
    private static string Move => AbilityConfigs.MoveStraight3Step.Id;

    [Test]
    public void Move_RecordsEachSquare()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.GroundedKit) }, new[] { (4, TestContent.Tough) });
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, Move, 0, new[] { 2 })));
        var moves = executor.State.Moves;
        Assert.AreEqual(2, moves.Count);
        Assert.AreEqual(0, moves[0].From.Dim1);
        Assert.AreEqual(1, moves[0].To.Dim1);
        Assert.AreEqual(2, moves[1].To.Dim1);
    }

    [Test]
    public void Move_PathGoesAroundBlocker()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (0, TestContent.GroundedKit) }, new[] { (1, TestContent.Tough) });
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, Move, 0, new[] { 4 })));
        var moves = executor.State.Moves;
        Assert.AreEqual(2, moves.Count);
        Assert.AreEqual(3, moves[0].To.Dim1);
        Assert.AreEqual(4, moves[1].To.Dim1);
    }

    [Test]
    public void Push_RecordsEachSquare()
    {
        var executor = TestGame.Start(TestContent.ShieldBash, TestContent.Tough, player2Position: 1);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.ShieldBash, 0, new[] { 1 })));
        var moves = executor.State.Moves;
        Assert.AreEqual(2, moves.Count);
        Assert.AreEqual(moves[0].Uid, moves[1].Uid);
        Assert.AreEqual(1, moves[0].From.Dim1);
        Assert.AreEqual(3, moves[1].To.Dim1);
    }

    [Test]
    public void Moves_ClearedByNextCommand()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.GroundedKit) }, new[] { (4, TestContent.Tough) });
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, Move, 0, new[] { 2 })));
        Assert.IsTrue(executor.Apply(new EndTurnCommand(1)));
        Assert.AreEqual(0, executor.State.Moves.Count);
    }
}
