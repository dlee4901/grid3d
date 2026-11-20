using NUnit.Framework;

public class KeywordTests
{
    private static bool Cast(TurnExecutor executor, string ability, int source, int target)
        => executor.Apply(new AbilityCommand(1, ability, source, new[] { target }));

    private static TurnExecutor Start(string kit)
        => TestGame.StartOn(3, 3, new[] { (4, kit) }, new[] { (5, TestContent.Tough) }, manaPerTurn: 5);

    [Test]
    public void Finisher_CastAfterOtherAbility()
    {
        var executor = Start(TestContent.DashKit);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 4, 5));
        Assert.IsTrue(Cast(executor, TestContent.Dash, 4, 3));
    }

    [Test]
    public void Finisher_NotTwice()
    {
        var executor = Start(TestContent.DashKit);
        Assert.IsTrue(Cast(executor, TestContent.Dash, 4, 3));
        Assert.IsFalse(Cast(executor, TestContent.Dash, 3, 0));
    }

    [Test]
    public void Finisher_BlocksOthersAfterIt()
    {
        var executor = Start(TestContent.DashKit);
        Assert.IsTrue(Cast(executor, TestContent.Dash, 4, 1));
        Assert.IsFalse(Cast(executor, TestContent.Hit3, 1, 2));
    }

    [Test]
    public void Double_CastTwice_NotThrice()
    {
        var executor = Start(TestContent.DoubleKit);
        Assert.IsTrue(Cast(executor, TestContent.Double, 4, 5));
        Assert.IsTrue(Cast(executor, TestContent.Double, 4, 5));
        Assert.IsFalse(Cast(executor, TestContent.Double, 4, 5));
    }

    [Test]
    public void Double_NotAfterOtherAbility()
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

    private static int CooldownOf(TurnExecutor executor, int position, string ability)
    {
        executor.State.GetEntity(position).TryGetComponent<AbilityComponent>(out var abilities);
        return abilities.Dictionary[ability].Cooldown;
    }

    [Test]
    public void Cast_PutsOtherAbilitiesOnCooldown1_ReadyNextTurn()
    {
        var executor = Start(TestContent.GroundedKit);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 4, 5));
        Assert.AreEqual(1, CooldownOf(executor, 4, AbilityConfigs.MoveStraight3Step.Id));
        Assert.IsTrue(executor.Apply(new EndTurnCommand(1)));
        Assert.AreEqual(0, CooldownOf(executor, 4, AbilityConfigs.MoveStraight3Step.Id));
    }

    [Test]
    public void Finisher_SkipsSiblingCooldown()
    {
        var executor = Start(TestContent.DashKit);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 4, 5));
        Assert.AreEqual(0, CooldownOf(executor, 4, TestContent.Dash));
    }

    [Test]
    public void Opener_LeavesOtherAbilitiesReady()
    {
        var executor = Start(TestContent.LeadKit);
        Assert.IsTrue(Cast(executor, TestContent.Lead, 4, 1));
        Assert.AreEqual(0, CooldownOf(executor, 1, TestContent.Hit3));
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 1, 2));
    }

    [Test]
    public void Opener_StillGoesOnItsOwnCooldown()
    {
        var executor = Start(TestContent.LeadKit);
        Assert.IsTrue(Cast(executor, TestContent.Lead, 4, 1));
        Assert.IsFalse(Cast(executor, TestContent.Lead, 1, 0));
    }

    [Test]
    public void PlainMove_AttackThenMove_Rejected()
    {
        var executor = Start(TestContent.GroundedKit);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 4, 5));
        Assert.IsFalse(Cast(executor, AbilityConfigs.MoveStraight3Step.Id, 4, 3));
    }
}
