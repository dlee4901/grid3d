using NUnit.Framework;

public class PreviewTests
{
    [SetUp]
    public void ResetCounters() => TestContent.ResetCounters();

    private static AbilityCommand Hit() => new(1, TestContent.KillThenCount, TestGame.Player1Position, new[] { 1 });

    [Test]
    public void Preview_DoesNotMutateState()
    {
        var executor = TestGame.Start(TestContent.KillThenCount, TestContent.Cheap, player2Position: 1);
        var hash = executor.State.ComputeHash();
        var logged = executor.Log.Count;
        Assert.IsNotNull(executor.Preview(Hit()));
        Assert.AreEqual(hash, executor.State.ComputeHash());
        Assert.AreEqual(logged, executor.Log.Count);
        Assert.IsNotNull(executor.State.GetEntity(1));
    }

    [Test]
    public void Preview_MatchesApply()
    {
        var executor = TestGame.Start(TestContent.KillThenCount, TestContent.Cheap, player2Position: 1);
        var preview = executor.Preview(Hit());
        Assert.IsTrue(executor.Apply(Hit()));
        Assert.AreEqual(executor.State.ComputeHash(), preview.ComputeHash());
    }

    [Test]
    public void Preview_InvalidCommand_ReturnsNull()
    {
        var executor = TestGame.Start(TestContent.KillThenCount, TestContent.Cheap, player2Position: 1);
        Assert.IsNull(executor.Preview(new AbilityCommand(2, TestContent.Cheap, 1, new[] { 2 })));
        Assert.IsNull(executor.Preview(new AbilityCommand(1, TestContent.KillThenCount, TestGame.Player1Position, new[] { 3 })));
    }

    [Test]
    public void Preview_KeepsUids()
    {
        var executor = TestGame.Start(TestContent.Step, TestContent.Cheap);
        var uid = executor.State.GetEntity(TestGame.Player1Position).Uid;
        Assert.AreNotEqual(0, uid);
        var preview = executor.Preview(new AbilityCommand(1, TestContent.Step, TestGame.Player1Position, new[] { 1 }));
        Assert.IsNotNull(preview);
        Assert.IsNull(preview.GetEntity(TestGame.Player1Position));
        Assert.AreEqual(uid, preview.GetEntity(1).Uid);
        Assert.AreEqual(uid, preview.GetEntityByUid(uid).Uid);
    }
}
