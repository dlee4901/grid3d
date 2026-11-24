using NUnit.Framework;

public class KeywordTests
{
    private static bool Cast(TurnExecutor executor, string ability, int source, int target)
        => executor.Apply(new AbilityCommand(1, ability, source, new[] { target }));

    private static TurnExecutor Start(string kit)
        => TestGame.StartOn(3, 3, new[] { (4, kit) }, new[] { (5, TestContent.Tough) });

    private static int CooldownOf(TurnExecutor executor, int position, string ability)
    {
        executor.State.GetEntity(position).TryGetComponent<AbilityComponent>(out var abilities);
        return abilities.Dictionary[ability].Cooldown;
    }

    [Test]
    public void PlainAbility_EndsTurn()
    {
        var executor = Start(TestContent.GroundedKit);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 4, 5));
        Assert.AreEqual(2, executor.State.ActivePlayer);
    }

    [Test]
    public void PlainMove_EndsTurn()
    {
        var executor = Start(TestContent.GroundedKit);
        Assert.IsTrue(Cast(executor, AbilityConfigs.MoveStraight3Step.Id, 4, 3));
        Assert.AreEqual(2, executor.State.ActivePlayer);
        Assert.IsFalse(Cast(executor, TestContent.Hit3, 3, 0));
    }

    [Test]
    public void Opener_DoesNotEndTurn()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (4, TestContent.LeadKit) }, new[] { (2, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, TestContent.Lead, 4, 1));
        Assert.AreEqual(1, executor.State.ActivePlayer);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 1, 2));
        Assert.AreEqual(2, executor.State.ActivePlayer);
    }

    [Test]
    public void Opener_OnlyCasterCanActAgain()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (4, TestContent.LeadKit), (0, TestContent.LeadKit) },
                                        new[] { (8, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, TestContent.Lead, 4, 5));
        Assert.AreEqual(1, executor.State.ActivePlayer);
        Assert.IsFalse(Cast(executor, TestContent.Lead, 0, 1));
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 5, 8));
        Assert.AreEqual(2, executor.State.ActivePlayer);
    }

    [Test]
    public void Opener_EndsTurnWhenNotFirstCast()
    {
        var executor = Start(TestContent.DoubleOpenerKit);
        Assert.IsTrue(Cast(executor, TestContent.Lead, 4, 1));
        Assert.AreEqual(1, executor.State.ActivePlayer);
        Assert.IsTrue(Cast(executor, TestContent.Lunge, 1, 0));
        Assert.AreEqual(2, executor.State.ActivePlayer);
    }

    [Test]
    public void Opener_StillGoesOnItsOwnCooldown()
    {
        var executor = Start(TestContent.LeadKit);
        Assert.IsTrue(Cast(executor, TestContent.Lead, 4, 1));
        Assert.IsFalse(Cast(executor, TestContent.Lead, 1, 0));
    }

    [Test]
    public void Opener_EndsTurnWhenNothingElseIsCastable()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (4, TestContent.LeadOnlyKit) }, new[] { (8, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, TestContent.Lead, 4, 1));
        Assert.AreEqual(2, executor.State.ActivePlayer);
    }

    [Test]
    public void Cooldown1_ReadyOnNextOwnTurn()
    {
        var executor = Start(TestContent.GroundedKit);
        Assert.IsTrue(Cast(executor, TestContent.Hit3, 4, 5));
        Assert.AreEqual(0, CooldownOf(executor, 4, TestContent.Hit3));
        Assert.AreEqual(0, CooldownOf(executor, 4, AbilityConfigs.MoveStraight3Step.Id));
    }
}
