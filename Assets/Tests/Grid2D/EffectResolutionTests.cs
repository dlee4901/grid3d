using NUnit.Framework;

public class EffectResolutionTests
{
    [SetUp]
    public void ResetCounters() => TestContent.ResetCounters();

    [Test]
    public void EffectOnLivingTarget_IsApplied()
    {
        var executor = TestGame.Start(TestContent.Count, TestContent.Cheap, player2Position: 1);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.Count, TestGame.Player1Position, new[] { 1 })));
        Assert.AreEqual(1, TestContent.Counter.Hits);
    }

    [Test]
    public void TargetKilledEarlierInCast_IsSkippedByLaterEffects()
    {
        var executor = TestGame.Start(TestContent.KillThenCount, TestContent.Cheap, player2Position: 1);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.KillThenCount, TestGame.Player1Position, new[] { 1 })));
        Assert.IsNull(executor.State.GetEntity(1));
        Assert.AreEqual(0, TestContent.Counter.Hits);
    }

    [Test]
    public void AllyOnlyEffect_SkipsEnemy()
    {
        var executor = TestGame.Start(TestContent.CountAllies, TestContent.Cheap, player2Position: 1);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.CountAllies, TestGame.Player1Position, new[] { 1 })));
        Assert.AreEqual(0, TestContent.AllyCounter.Hits);
    }

    [Test]
    public void AreaEffect_ReachesCasterAndEnemy()
    {
        var executor = TestGame.Start(TestContent.BlastAll, TestContent.Cheap, player2Position: 1);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.BlastAll, TestGame.Player1Position, new[] { 1 })));
        Assert.AreEqual(2, TestContent.Counter.Hits);
    }

    [Test]
    public void AreaEffectExcludingSelf_SkipsCaster()
    {
        var executor = TestGame.Start(TestContent.BlastNotSelf, TestContent.Cheap, player2Position: 1);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.BlastNotSelf, TestGame.Player1Position, new[] { 1 })));
        Assert.AreEqual(1, TestContent.NotSelfCounter.Hits);
    }
}
