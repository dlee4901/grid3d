using NUnit.Framework;

public class CastabilityTests
{
    [Test]
    public void AffordableAbility_IsCastable()
    {
        var state = TestGame.Start(TestContent.Cheap, TestContent.Cheap).State;
        Assert.IsTrue(state.CanCast(state.GetEntity(TestGame.Player1Position), TestGame.FirstAbilityAt(state, TestGame.Player1Position), 1));
    }

    [Test]
    public void AbilityCostingMoreThanMana_IsNotCastable()
    {
        var state = TestGame.Start(TestContent.Expensive, TestContent.Cheap).State;
        Assert.IsFalse(state.CanCast(state.GetEntity(TestGame.Player1Position), TestGame.FirstAbilityAt(state, TestGame.Player1Position), 1));
    }

    [Test]
    public void LockedAbility_IsNotCastable()
    {
        var state = TestGame.Start(TestContent.Locked, TestContent.Cheap).State;
        Assert.IsFalse(state.CanCast(state.GetEntity(TestGame.Player1Position), TestGame.FirstAbilityAt(state, TestGame.Player1Position), 1));
    }

    [Test]
    public void AbilityInWarmup_IsNotCastable()
    {
        var state = TestGame.Start(TestContent.WarmingUp, TestContent.Cheap).State;
        Assert.IsFalse(state.CanCast(state.GetEntity(TestGame.Player1Position), TestGame.FirstAbilityAt(state, TestGame.Player1Position), 1));
    }
}
