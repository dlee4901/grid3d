using NUnit.Framework;

public class CommandAuthorityTests
{
    [Test]
    public void OffTurnCommand_IsRejected()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap);
        Assert.IsFalse(executor.Apply(new EndTurnCommand(2)));
        Assert.AreEqual(1, executor.State.ActivePlayer);
    }

    [Test]
    public void CastingWithOpponentsUnit_IsRejected()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap);
        var command = new AbilityCommand(1, TestContent.Cheap, TestGame.Player2Position,
                                         new[] { TestGame.Player2Position });
        Assert.IsFalse(executor.Apply(command));
        Assert.AreEqual(3, executor.State.GetMana(1));
    }
}
