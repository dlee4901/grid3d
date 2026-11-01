using NUnit.Framework;

public class DeterminismTests
{
    [Test]
    public void SameCommandSequence_ProducesSameState()
    {
        var a = TestGame.Start(TestContent.Cheap, TestContent.Cheap);
        var b = TestGame.Start(TestContent.Cheap, TestContent.Cheap);
        foreach (var executor in new[] { a, b })
        {
            executor.Apply(new EndTurnCommand(1, 1500));
            executor.Apply(new EndTurnCommand(2, 2500));
            executor.Apply(new EndTurnCommand(1, 500));
        }

        Assert.AreEqual(a.State.PrintGrid(), b.State.PrintGrid());
        Assert.AreEqual(a.State.Turn, b.State.Turn);
        Assert.AreEqual(a.State.ActivePlayer, b.State.ActivePlayer);
        for (var player = 1; player <= 2; player++)
        {
            Assert.AreEqual(a.State.GetMana(player), b.State.GetMana(player));
            Assert.AreEqual(a.State.GetTimeMs(player), b.State.GetTimeMs(player));
        }
    }
}
