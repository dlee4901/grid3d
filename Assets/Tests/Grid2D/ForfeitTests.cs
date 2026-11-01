using NUnit.Framework;

public class ForfeitTests
{
    [Test]
    public void Forfeit_GivesTheOtherPlayerTheWin()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap);
        Assert.IsTrue(executor.Apply(new ForfeitCommand(1)));
        Assert.AreEqual(GameOutcome.Victory, executor.State.Outcome);
        Assert.AreEqual(2, executor.State.Winner);
    }

    [Test]
    public void Forfeit_WorksOutOfTurn()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap);
        Assert.IsTrue(executor.Apply(new ForfeitCommand(2)));
        Assert.AreEqual(1, executor.State.Winner);
    }

    [Test]
    public void AfterForfeit_TurnCommandsAreRejected()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap);
        executor.Apply(new ForfeitCommand(2));
        Assert.IsFalse(executor.Apply(new EndTurnCommand(1)));
    }

    [Test]
    public void SecondForfeit_IsRejected()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap);
        executor.Apply(new ForfeitCommand(2));
        Assert.IsFalse(executor.Apply(new ForfeitCommand(1)));
        Assert.AreEqual(1, executor.State.Winner);
    }

    [Test]
    public void ForfeitAfterDraw_IsRejected()
    {
        var executor = TestGame.Start(TestContent.Expensive, TestContent.Expensive);
        Assert.IsFalse(executor.Apply(new ForfeitCommand(1)));
        Assert.AreEqual(GameOutcome.Draw, executor.State.Outcome);
    }

    [Test]
    public void Forfeit_ChangesTheStateHash()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap);
        var before = executor.StateHash;
        executor.Apply(new ForfeitCommand(1));
        Assert.AreNotEqual(before, executor.StateHash);
    }
}
