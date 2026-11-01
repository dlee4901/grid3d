using NUnit.Framework;

public class TargetValidationTests
{
    private static AbilityCommand Move(int from, params int[] to)
        => new AbilityCommand(1, TestContent.Step, from, to);

    [Test]
    public void MoveWithinRange_MovesUnit()
    {
        var executor = TestGame.Start(TestContent.Step, TestContent.Cheap);
        Assert.IsTrue(executor.Apply(Move(TestGame.Player1Position, 1)));
        Assert.IsNull(executor.State.GetEntity(TestGame.Player1Position));
        Assert.IsNotNull(executor.State.GetEntity(1));
        Assert.AreEqual(2, executor.State.GetMana(1));
    }

    [Test]
    public void MoveOutOfRange_IsRejected()
    {
        var executor = TestGame.Start(TestContent.Step, TestContent.Cheap);
        Assert.IsFalse(executor.Apply(Move(TestGame.Player1Position, 2)));
        Assert.IsNotNull(executor.State.GetEntity(TestGame.Player1Position));
        Assert.AreEqual(3, executor.State.GetMana(1));
    }

    [Test]
    public void MoveOntoOccupiedTile_IsRejected()
    {
        var executor = TestGame.Start(TestContent.Step, TestContent.Cheap, player2Position: 1);
        Assert.IsFalse(executor.Apply(Move(TestGame.Player1Position, 1)));
        Assert.IsNotNull(executor.State.GetEntity(TestGame.Player1Position));
        Assert.IsNotNull(executor.State.GetEntity(1));
        Assert.AreEqual(2, executor.State.ResolutionOrder.Entities.Count);
    }

    [Test]
    public void MoveWithoutTarget_IsRejected()
    {
        var executor = TestGame.Start(TestContent.Step, TestContent.Cheap);
        Assert.IsFalse(executor.Apply(Move(TestGame.Player1Position)));
        Assert.IsNotNull(executor.State.GetEntity(TestGame.Player1Position));
    }

    [Test]
    public void ChangeEntityPositionOntoItself_IsRejected()
    {
        var state = TestGame.Start(TestContent.Step, TestContent.Cheap).State;
        var position = new GridPosition(state, TestGame.Player1Position);
        Assert.IsFalse(state.ChangeEntityPosition(position, position));
        Assert.IsNotNull(state.GetEntity(TestGame.Player1Position));
    }

    [Test]
    public void TargetedAbilityWithoutTarget_IsRejected()
    {
        var executor = TestGame.Start(TestContent.Poke, TestContent.Cheap);
        Assert.IsFalse(executor.Apply(new AbilityCommand(1, TestContent.Poke, TestGame.Player1Position, new int[0])));
        Assert.AreEqual(3, executor.State.GetMana(1));
    }
}
