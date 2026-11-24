using NUnit.Framework;

public class ForcedPassTests
{
    [Test]
    public void PlayerWithNothingCastable_IsSkippedAtGameStart()
    {
        var state = TestGame.Start(TestContent.WarmingUp, TestContent.Cheap).State;
        Assert.AreEqual(2, state.ActivePlayer);
        Assert.AreEqual(GameOutcome.InProgress, state.Outcome);
    }

    [Test]
    public void SkippedPlayer_ActsAgainOnceWarmupEnds()
    {
        var executor = TestGame.Start(TestContent.WarmingUp, TestContent.Cheap);
        Assert.IsTrue(executor.Apply(new PassCommand(2)));
        Assert.AreEqual(1, executor.State.ActivePlayer);
    }

    [Test]
    public void NobodyCanAct_EndsInDraw()
    {
        var state = TestGame.Start(TestContent.Locked, TestContent.Locked).State;
        Assert.AreEqual(GameOutcome.Draw, state.Outcome);
    }

    [Test]
    public void AfterDraw_TurnCommandsAreRejected()
    {
        var executor = TestGame.Start(TestContent.Locked, TestContent.Locked);
        Assert.IsFalse(executor.Apply(new PassCommand(executor.State.ActivePlayer)));
    }

    [Test]
    public void OnePlayerSkippedEveryCycle_DoesNotDraw()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Locked);
        for (var cycle = 0; cycle < 3; cycle++)
        {
            Assert.IsTrue(executor.Apply(new PassCommand(1)));
            Assert.AreEqual(1, executor.State.ActivePlayer);
        }
        Assert.AreEqual(GameOutcome.InProgress, executor.State.Outcome);
    }
}
