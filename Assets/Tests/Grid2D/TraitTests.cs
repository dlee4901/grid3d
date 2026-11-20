using NUnit.Framework;

public class TraitTests
{
    private static Entity At(TurnExecutor executor, int position) => executor.State.GetEntity(position);

    private static int Health(TurnExecutor executor, int position)
    {
        At(executor, position).TryGetComponent<HealthComponent>(out var health);
        return health.Current;
    }

    private static int Count(TurnExecutor executor, int position, string statusId)
    {
        At(executor, position).TryGetComponent<TraitComponent>(out var traits);
        foreach (var status in traits.Statuses) if (status.Id == statusId) return status.Count;
        return 0;
    }

    private static void Cast(TurnExecutor executor, int player, string ability, int source, int target)
        => Assert.IsTrue(executor.Apply(new AbilityCommand(player, ability, source, new[] { target })));

    private static void EndTurn(TurnExecutor executor, int player)
        => Assert.IsTrue(executor.Apply(new EndTurnCommand(player)));

    [Test]
    public void Burn_TicksOnOwnersTurnEnd_ThenExpires()
    {
        var executor = TestGame.Start(TestContent.BurnTouch, TestContent.Tough, player2Position: 1);
        Cast(executor, 1, TestContent.BurnTouch, 0, 1);
        Assert.AreEqual(1, Count(executor, 1, "Burn"));
        EndTurn(executor, 1);
        Assert.AreEqual(10, Health(executor, 1));
        Assert.AreEqual(1, Count(executor, 1, "Burn"));
        EndTurn(executor, 2);
        Assert.AreEqual(9, Health(executor, 1));
        Assert.AreEqual(0, Count(executor, 1, "Burn"));
    }

    [Test]
    public void Blaze_BlocksBurn_GainsPower()
    {
        var executor = TestGame.Start(TestContent.BurnTouch, TestContent.Blazing, player2Position: 1);
        Cast(executor, 1, TestContent.BurnTouch, 0, 1);
        EndTurn(executor, 1);
        EndTurn(executor, 2);
        Assert.AreEqual(10, Health(executor, 1));
        Assert.AreEqual(1, Count(executor, 1, "Power"));
        Assert.AreEqual(0, Count(executor, 1, "Burn"));
    }

    [Test]
    public void Armored_ReducesEachHitByOne()
    {
        var executor = TestGame.Start(TestContent.Hit3, TestContent.Armored, player2Position: 1);
        Cast(executor, 1, TestContent.Hit3, 0, 1);
        Assert.AreEqual(8, Health(executor, 1));
    }

    [Test]
    public void Armored_DoesNotReduceBurn()
    {
        var executor = TestGame.Start(TestContent.BurnTouch, TestContent.Armored, player2Position: 1);
        Cast(executor, 1, TestContent.BurnTouch, 0, 1);
        EndTurn(executor, 1);
        EndTurn(executor, 2);
        Assert.AreEqual(9, Health(executor, 1));
    }

    [Test]
    public void DamageBreakdown_ListsPowerAsModifier_WithoutChangingState()
    {
        var executor = TestGame.Start(TestContent.PowerHit, TestContent.Tough, player2Position: 1);
        Cast(executor, 1, TestContent.PowerHit, 0, 1);
        var caster = At(executor, 0);
        var hash = executor.State.ComputeHash();
        var roll = DamageBreakdown.ForCaster(executor.State, caster, 3);
        Assert.AreEqual(3, roll.BaseAmount);
        Assert.AreEqual(5, roll.Amount);
        Assert.AreEqual(1, roll.Modifiers.Count);
        Assert.AreEqual(("Power", 2), roll.Modifiers[0]);
        Assert.AreEqual(hash, executor.State.ComputeHash());
    }

    [Test]
    public void Power_AddsCountToDamageDealt()
    {
        var executor = TestGame.Start(TestContent.PowerHit, TestContent.Tough, player2Position: 1);
        Cast(executor, 1, TestContent.PowerHit, 0, 1);
        Assert.AreEqual(2, Count(executor, 0, "Power"));
        Assert.AreEqual(7, Health(executor, 1));
    }

    [Test]
    public void ShieldBash_Damages_AndPushesTwo()
    {
        var executor = TestGame.Start(TestContent.ShieldBash, TestContent.Tough, player2Position: 1);
        Cast(executor, 1, TestContent.ShieldBash, 0, 1);
        Assert.IsNull(At(executor, 1));
        Assert.AreEqual(8, Health(executor, 3));
    }

    [Test]
    public void Push_StopsAtBorder()
    {
        var executor = TestGame.Start(TestContent.Shove, TestContent.Tough, player2Position: 1);
        Cast(executor, 1, TestContent.Shove, 0, 1);
        Assert.IsNotNull(At(executor, 4));
    }

    [Test]
    public void Push_StopsAtUnit()
    {
        var executor = TestGame.StartOn(5, 1,
            new[] { (0, TestContent.Shove) },
            new[] { (1, TestContent.Tough), (3, TestContent.Tough) });
        Cast(executor, 1, TestContent.Shove, 0, 1);
        Assert.IsNull(At(executor, 1));
        Assert.IsNotNull(At(executor, 2));
        Assert.IsNotNull(At(executor, 3));
    }

    [Test]
    public void SwordSlash_HitsSideSquareAndBothDiagonals()
    {
        var executor = TestGame.StartOn(3, 3,
            new[] { (3, TestContent.SwordSlash) },
            new[] { (1, TestContent.Tough), (4, TestContent.Tough), (7, TestContent.Tough), (5, TestContent.Tough) });
        Cast(executor, 1, TestContent.SwordSlash, 3, 4);
        Assert.AreEqual(6, Health(executor, 1));
        Assert.AreEqual(6, Health(executor, 4));
        Assert.AreEqual(6, Health(executor, 7));
        Assert.AreEqual(10, Health(executor, 5));
    }

    [Test]
    public void Retaliate_HitsAdjacentAttacker()
    {
        var executor = TestGame.Start(TestContent.Hit3, TestContent.Retaliator, player2Position: 1);
        Cast(executor, 1, TestContent.Hit3, 0, 1);
        Assert.AreEqual(7, Health(executor, 1));
        Assert.IsNull(At(executor, 0));
    }

    [Test]
    public void Retaliate_IgnoresDistantAttacker()
    {
        var executor = TestGame.Start(TestContent.Axe, TestContent.Retaliator, player2Position: 3);
        Cast(executor, 1, TestContent.Axe, 0, 1);
        Assert.AreEqual(7, Health(executor, 3));
        Assert.IsNotNull(At(executor, 0));
    }

    [Test]
    public void Retaliate_IgnoresDiagonalAttacker()
    {
        var executor = TestGame.StartOn(3, 3,
            new[] { (0, TestContent.SwordSlash) },
            new[] { (4, TestContent.Retaliator) });
        Cast(executor, 1, TestContent.SwordSlash, 0, 1);
        Assert.AreEqual(6, Health(executor, 4));
        Assert.AreEqual(10, Health(executor, 0));
    }

    [Test]
    public void Died_TriggersOnDeath_AndChains()
    {
        var executor = TestGame.Start(TestContent.Avenger, TestContent.Avenger, player2Position: 1);
        Cast(executor, 1, TestContent.Avenger, 0, 1);
        Assert.IsNull(At(executor, 1));
        Assert.IsNull(At(executor, 0));
    }

    [Test]
    public void DeathExplosion_HitsAdjacent_AndChains()
    {
        var executor = TestGame.StartOn(5, 1,
            new[] { (0, TestContent.Hit3) },
            new[] { (1, TestContent.Volatile), (2, TestContent.Volatile), (3, TestContent.Tough) });
        Cast(executor, 1, TestContent.Hit3, 0, 1);
        Assert.IsNull(At(executor, 0));
        Assert.IsNull(At(executor, 1));
        Assert.IsNull(At(executor, 2));
        Assert.AreEqual(8, Health(executor, 3));
    }

    [Test]
    public void Feast_GainsPower_WhenUnitDiesWithinRange()
    {
        var executor = TestGame.StartOn(6, 1,
            new[] { (0, TestContent.Hit3), (5, TestContent.Scavenger) },
            new[] { (1, TestContent.Cheap), (4, TestContent.Scavenger) });
        Cast(executor, 1, TestContent.Hit3, 0, 1);
        Assert.AreEqual(1, Count(executor, 4, "Power"));
        Assert.AreEqual(0, Count(executor, 5, "Power"));
    }

    [Test]
    public void Statuses_ChangeStateHash()
    {
        var state = TestGame.Start(TestContent.Tough, TestContent.Tough, player2Position: 1).State;
        state.GetEntity(1).TryGetComponent<TraitComponent>(out var traits);
        var before = state.ComputeHash();
        traits.AdjustStatus(StatusConfigs.Burn, 1);
        Assert.AreNotEqual(before, state.ComputeHash());
        traits.AdjustStatus(StatusConfigs.Burn, -1);
        Assert.AreEqual(before, state.ComputeHash());
    }
}
