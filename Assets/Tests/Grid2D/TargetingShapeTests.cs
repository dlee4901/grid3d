using NUnit.Framework;

public class TargetingShapeTests
{
    [SetUp]
    public void ResetCounters() => TestContent.ResetCounters();

    private static GridSteps BoltStepsEast(TurnExecutor executor)
    {
        var state = executor.State;
        var source = new GridSource(state, new GridPosition(state, TestGame.Player1Position), state.GetEntity(TestGame.Player1Position));
        var ability = TestGame.FirstAbilityAt(state, TestGame.Player1Position);
        Assert.IsTrue(ability.Targeting.GetEffectSteps(source, new GridPosition(state, 1), out var steps));
        return steps;
    }

    private static bool Contains(TurnExecutor executor, GridSteps steps, int position)
        => steps.Contains(new GridPosition(executor.State, position));

    [Test]
    public void Bolt_StopsOnFirstUnit()
    {
        var executor = TestGame.Start(TestContent.Bolt, TestContent.Cheap, player2Position: 2);
        var steps = BoltStepsEast(executor);
        Assert.IsTrue(Contains(executor, steps, 1));
        Assert.IsTrue(Contains(executor, steps, 2));
        Assert.IsFalse(Contains(executor, steps, 3));
        Assert.IsFalse(Contains(executor, steps, 4));
    }

    [Test]
    public void Bolt_HitsBlocker()
    {
        var executor = TestGame.Start(TestContent.Bolt, TestContent.Cheap, player2Position: 2);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.Bolt, TestGame.Player1Position, new[] { 1 })));
        Assert.AreEqual(1, TestContent.Counter.Hits);
    }

    [Test]
    public void Bolt_FarUnit_KeepsPath()
    {
        var executor = TestGame.Start(TestContent.Bolt, TestContent.Cheap);
        var steps = BoltStepsEast(executor);
        for (var position = 1; position <= 4; position++)
            Assert.IsTrue(Contains(executor, steps, position), $"missing {position}");
    }

    private static GridSteps AdjacentWalk(TurnExecutor executor, Occupant passthrough)
    {
        var state = executor.State;
        var source = new GridSource(state, new GridPosition(state, TestGame.Player1Position), state.GetEntity(TestGame.Player1Position));
        var selection = new GridSelection
        {
            Traversals = new System.Collections.Generic.List<GridTraversal>
            {
                new GridTraversal { Direction = GridDirection.Straight, Linear = false, MaxDistance = 4, Passthrough = passthrough }
            }
        };
        return selection.GetGridSteps(source);
    }

    [Test]
    public void Passthrough_Units_WalksAcrossTouchingUnit()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap, player2Position: 1);
        var steps = AdjacentWalk(executor, Occupant.Units);
        Assert.IsTrue(Contains(executor, steps, 0));
        Assert.IsTrue(Contains(executor, steps, 1));
        Assert.IsFalse(Contains(executor, steps, 2));
    }

    [Test]
    public void Passthrough_Units_StopsAtEmpty()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap, player2Position: 2);
        var steps = AdjacentWalk(executor, Occupant.Units);
        Assert.IsTrue(Contains(executor, steps, 0));
        for (var position = 1; position <= 4; position++)
            Assert.IsFalse(Contains(executor, steps, position), $"unexpected {position}");
    }

    [Test]
    public void Passthrough_EmptyDefault_BlocksUnit()
    {
        var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap, player2Position: 2);
        var steps = AdjacentWalk(executor, Occupant.Empty);
        Assert.IsTrue(Contains(executor, steps, 0));
        Assert.IsTrue(Contains(executor, steps, 1));
        for (var position = 2; position <= 4; position++)
            Assert.IsFalse(Contains(executor, steps, position), $"unexpected {position}");
    }

    [Test]
    public void Pair_DuplicateTarget_IsRejected()
    {
        var executor = TestGame.Start(TestContent.Pair, TestContent.Cheap);
        Assert.IsFalse(executor.Apply(new AbilityCommand(1, TestContent.Pair, TestGame.Player1Position, new[] { 4, 4 })));
        Assert.AreEqual(3, executor.State.GetMana(1));
        Assert.AreEqual(0, TestContent.Counter.Hits);
    }

    [Test]
    public void Pair_DistinctUnits_Applies()
    {
        var executor = TestGame.Start(TestContent.Pair, TestContent.Cheap);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.Pair, TestGame.Player1Position, new[] { 0, 4 })));
        Assert.AreEqual(2, TestContent.Counter.Hits);
    }

    [Test]
    public void Pair_EmptyTarget_IsRejected()
    {
        var executor = TestGame.Start(TestContent.Pair, TestContent.Cheap);
        Assert.IsFalse(executor.Apply(new AbilityCommand(1, TestContent.Pair, TestGame.Player1Position, new[] { 3, 4 })));
        Assert.AreEqual(3, executor.State.GetMana(1));
    }

    [Test]
    public void Stab_EmptySquare_IsRejected()
    {
        var executor = TestGame.Start(TestContent.StabEnemy, TestContent.Cheap);
        Assert.IsFalse(executor.Apply(new AbilityCommand(1, TestContent.StabEnemy, TestGame.Player1Position, new[] { 1 })));
        Assert.AreEqual(3, executor.State.GetMana(1));
    }

    [Test]
    public void Stab_Enemy_Applies()
    {
        var executor = TestGame.Start(TestContent.StabEnemy, TestContent.Cheap, player2Position: 1);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.StabEnemy, TestGame.Player1Position, new[] { 1 })));
        Assert.AreEqual(1, TestContent.Counter.Hits);
    }

    private static int HealthAt(TurnExecutor executor, int position)
    {
        executor.State.GetEntity(position).TryGetComponent<HealthComponent>(out var health);
        return health.Current;
    }

    [Test]
    public void SelfHeal_RestoresHealth()
    {
        var executor = TestGame.Start(TestContent.SelfHeal, TestContent.KillThenCount, player2Position: 1);
        Assert.IsTrue(executor.Apply(new EndTurnCommand(1)));
        Assert.IsTrue(executor.Apply(new AbilityCommand(2, TestContent.KillThenCount, 1, new[] { TestGame.Player1Position })));
        Assert.AreEqual(2, HealthAt(executor, TestGame.Player1Position));
        Assert.IsTrue(executor.Apply(new EndTurnCommand(2)));
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.SelfHeal, TestGame.Player1Position, new[] { TestGame.Player1Position })));
        Assert.AreEqual(3, HealthAt(executor, TestGame.Player1Position));
    }

    [Test]
    public void SelfHeal_CapsAtStarting()
    {
        var executor = TestGame.Start(TestContent.SelfHeal, TestContent.Cheap);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.SelfHeal, TestGame.Player1Position, new[] { TestGame.Player1Position })));
        Assert.AreEqual(3, HealthAt(executor, TestGame.Player1Position));
    }
}
