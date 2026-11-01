using System;
using System.Collections.Generic;
using NUnit.Framework;

public class StateHashTests
{
    private static TurnExecutor PlayShortGame()
    {
        var executor = TestGame.Start(TestContent.Step, TestContent.Cheap);
        executor.Apply(new AbilityCommand(1, TestContent.Step, TestGame.Player1Position, new[] { 1 }));
        executor.Apply(new EndTurnCommand(1, 1500));
        executor.Apply(new EndTurnCommand(2, 2500));
        executor.Apply(new AbilityCommand(1, TestContent.Step, 1, new[] { 2 }));
        return executor;
    }

    private static void AssertChangesHash(Action<GridState> mutate)
    {
        var state = TestGame.Start(TestContent.WarmingUp, TestContent.Cheap).State;
        var before = state.ComputeHash();
        mutate(state);
        Assert.AreNotEqual(before, state.ComputeHash());
    }

    [Test]
    public void SameCommandSequence_ProducesSameHash()
    {
        Assert.AreEqual(PlayShortGame().StateHash, PlayShortGame().StateHash);
    }

    [Test]
    public void ReplayingLog_ReproducesState()
    {
        var original = PlayShortGame();
        var replay = TurnExecutor.ForDefinition(original.State.Definition);
        foreach (var command in original.Log) Assert.IsTrue(replay.Apply(command));
        Assert.AreEqual(original.StateHash, replay.StateHash);
        Assert.AreEqual(original.State.PrintGrid(), replay.State.PrintGrid());
    }

    [Test]
    public void AppliedCommands_AreLoggedInOrder()
    {
        var executor = PlayShortGame();
        Assert.AreEqual(7, executor.Log.Count);
        Assert.IsInstanceOf<SpawnTeamsCommand>(executor.Log[2]);
        Assert.IsInstanceOf<AbilityCommand>(executor.Log[6]);
    }

    [Test]
    public void RejectedCommand_IsNotLogged_AndLeavesHashUnchanged()
    {
        var executor = TestGame.Start(TestContent.Step, TestContent.Cheap);
        var count = executor.Log.Count;
        var hash = executor.StateHash;
        Assert.IsFalse(executor.Apply(new EndTurnCommand(2)));
        Assert.AreEqual(count, executor.Log.Count);
        Assert.AreEqual(hash, executor.StateHash);
    }

    [Test]
    public void Hash_CoversMana() => AssertChangesHash(state => state.SpendMana(2, 1));

    [Test]
    public void Hash_CoversTimeBank() => AssertChangesHash(state => state.SpendTime(1, 1));

    [Test]
    public void Hash_CoversHealth() => AssertChangesHash(state =>
    {
        state.GetEntity(TestGame.Player2Position).TryGetComponent<HealthComponent>(out var health);
        health.Current = 0;
    });

    [Test]
    public void Hash_CoversCooldowns() => AssertChangesHash(state =>
        TestGame.FirstAbilityAt(state, TestGame.Player1Position).TurnUpdate());

    [Test]
    public void Hash_CoversPositions() => AssertChangesHash(state =>
        state.ChangeEntityPosition(new GridPosition(state, TestGame.Player2Position), new GridPosition(state, 3)));

    [Test]
    public void Hash_CoversTurn() => AssertChangesHash(state => state.AdvanceTurn());

    [Test]
    public void Hash_CoversResolutionOrder()
    {
        var state = TestGame.Start(TestContent.Cheap, TestContent.Cheap).State;
        var before = state.ComputeHash();
        state.ResolutionOrder.MoveToFront(state.ResolutionOrder.Entities[1]);
        Assert.AreNotEqual(before, state.ComputeHash());
    }

    [Test]
    public void MutationOutsideCommand_IsReported()
    {
        var errors = new List<string>();
        var previous = GridLog.Error;
        GridLog.Error = errors.Add;
        try
        {
            var executor = TestGame.Start(TestContent.Cheap, TestContent.Cheap);
            executor.State.SpendMana(1, 1);
            executor.Apply(new EndTurnCommand(1));
            Assert.AreEqual(1, errors.Count);
            StringAssert.Contains("outside a command", errors[0]);
        }
        finally
        {
            GridLog.Error = previous;
        }
    }
}
