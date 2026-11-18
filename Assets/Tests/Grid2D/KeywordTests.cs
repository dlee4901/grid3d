using NUnit.Framework;

public class KeywordTests
{
    private static bool Cast(TurnExecutor executor, string ability, int source, int target)
        => executor.Apply(new AbilityCommand(1, ability, source, new[] { target }));

    private static TurnExecutor Start(string kit)
        => TestGame.StartOn(3, 3, new[] { (4, kit) }, new[] { (5, TestContent.Tough) }, manaPerTurn: 5);

    [Test]
    public void Successive_CastAfterOtherAbility()
    {
        var executor = Start(TestContent.DashKit);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 4, 5));
        Assert.IsTrue(Cast(executor, TestContent.Dash, 4, 3));
    }

    [Test]
    public void Successive_NotTwice()
    {
        var executor = Start(TestContent.DashKit);
        Assert.IsTrue(Cast(executor, TestContent.Dash, 4, 3));
        Assert.IsFalse(Cast(executor, TestContent.Dash, 3, 0));
    }

    [Test]
    public void Successive_BlocksOthersAfterIt()
    {
        var executor = Start(TestContent.DashKit);
        Assert.IsTrue(Cast(executor, TestContent.Dash, 4, 1));
        Assert.IsFalse(Cast(executor, TestContent.Hit3, 1, 2));
    }

    [Test]
    public void Repeat_CastTwice_NotThrice()
    {
        var executor = Start(TestContent.DoubleKit);
        Assert.IsTrue(Cast(executor, TestContent.Double, 4, 5));
        Assert.IsTrue(Cast(executor, TestContent.Double, 4, 5));
        Assert.IsFalse(Cast(executor, TestContent.Double, 4, 5));
    }

    [Test]
    public void Repeat_NotAfterOtherAbility()
    {
        var executor = Start(TestContent.DoubleKit);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 4, 5));
        Assert.IsFalse(Cast(executor, TestContent.Double, 4, 5));
    }

    [Test]
    public void NimbleMove_AttackThenMoveTwice()
    {
        var executor = Start(TestContent.NimbleKit);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 4, 5));
        Assert.IsTrue(Cast(executor, AbilityConfigs.MoveNimble.Id, 4, 3));
        Assert.IsTrue(Cast(executor, AbilityConfigs.MoveNimble.Id, 3, 0));
        Assert.IsFalse(Cast(executor, AbilityConfigs.MoveNimble.Id, 0, 1));
    }

    [Test]
    public void NimbleMove_MoveThenAttack_Rejected()
    {
        var executor = Start(TestContent.NimbleKit);
        Assert.IsTrue(Cast(executor, AbilityConfigs.MoveNimble.Id, 4, 1));
        Assert.IsFalse(Cast(executor, TestContent.Hit3, 1, 2));
    }

    [Test]
    public void PlainMove_AttackThenMove_Rejected()
    {
        var executor = Start(TestContent.GroundedKit);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 4, 5));
        Assert.IsFalse(Cast(executor, AbilityConfigs.MoveStraight3Step.Id, 4, 3));
    }
}
