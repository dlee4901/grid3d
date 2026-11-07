using NUnit.Framework;

public class AbilityRulesTests
{
    [SetUp]
    public void ResetCounters() => TestContent.ResetCounters();

    private static AbilityCommand SwingEast() => new(1, TestContent.Axe, TestGame.Player1Position, new[] { 1 });

    private static int HealthAt(TurnExecutor executor, int position)
    {
        executor.State.GetEntity(position).TryGetComponent<HealthComponent>(out var health);
        return health.Current;
    }

    private static Ability AxeOf(TurnExecutor executor) => TestGame.FirstAbilityAt(executor.State, TestGame.Player1Position);

    [Test]
    public void Axe_Adjacent_Deals5_NoCooldown()
    {
        var executor = TestGame.Start(TestContent.Axe, TestContent.Tough, player2Position: 1);
        Assert.IsTrue(executor.Apply(SwingEast()));
        Assert.AreEqual(5, HealthAt(executor, 1));
        Assert.AreEqual(0, AxeOf(executor).Cooldown);
    }

    [Test]
    public void Axe_Far_Deals3_Cooldown2()
    {
        var executor = TestGame.Start(TestContent.Axe, TestContent.Tough, player2Position: 3);
        Assert.IsTrue(executor.Apply(SwingEast()));
        Assert.AreEqual(7, HealthAt(executor, 3));
        Assert.AreEqual(2, AxeOf(executor).Cooldown);
    }

    [Test]
    public void Axe_FarKill_StillCoolsDown()
    {
        var executor = TestGame.Start(TestContent.Axe, TestContent.Cheap, player2Position: 3);
        Assert.IsTrue(executor.Apply(SwingEast()));
        Assert.IsNull(executor.State.GetEntity(3));
        Assert.AreEqual(2, AxeOf(executor).Cooldown);
    }

    [Test]
    public void Axe_Cooldown_SkipsNextTurn()
    {
        var executor = TestGame.Start(TestContent.Axe, TestContent.Tough, player2Position: 3);
        Assert.IsTrue(executor.Apply(SwingEast()));
        Assert.IsTrue(executor.Apply(new EndTurnCommand(1)));
        Assert.IsTrue(executor.Apply(new EndTurnCommand(2)));
        var state = executor.State;
        Assert.AreEqual(1, AxeOf(executor).Cooldown);
        Assert.IsFalse(state.CanCast(state.GetEntity(TestGame.Player1Position), AxeOf(executor), 1));
    }

    [Test]
    public void OneAbilityPerTurn_SecondCastRejected()
    {
        var executor = TestGame.Start(TestContent.Poke, TestContent.Cheap);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.Poke, TestGame.Player1Position, new[] { 1 })));
        Assert.IsFalse(executor.Apply(new AbilityCommand(1, TestContent.Poke, TestGame.Player1Position, new[] { 1 })));
        Assert.AreEqual(2, executor.State.GetMana(1));
    }

    [Test]
    public void OneAbilityPerTurn_ResetsNextTurn()
    {
        var executor = TestGame.Start(TestContent.Poke, TestContent.Cheap);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.Poke, TestGame.Player1Position, new[] { 1 })));
        Assert.IsTrue(executor.Apply(new EndTurnCommand(1)));
        Assert.IsTrue(executor.Apply(new EndTurnCommand(2)));
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.Poke, TestGame.Player1Position, new[] { 1 })));
    }
}
