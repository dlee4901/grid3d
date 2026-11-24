using System;
using NUnit.Framework;

public class CommandCodecTests
{
    [Test]
    public void AbilityCommand_RoundTrips()
    {
        var bytes = CommandCodec.Encode(new AbilityCommand(1, "Fireball", 15, new[] { 37, 38 }));
        Assert.IsTrue(CommandCodec.TryDecode(bytes, 1, out var decoded));
        var ability = (AbilityCommand)decoded;
        Assert.AreEqual("Fireball", ability.AbilityId);
        Assert.AreEqual(15, ability.SourcePosition);
        CollectionAssert.AreEqual(new[] { 37, 38 }, ability.TargetPositions);
        Assert.IsNull(ability.Path);
    }

    [Test]
    public void AbilityCommand_WithPath_RoundTrips()
    {
        var bytes = CommandCodec.Encode(new AbilityCommand(1, "MoveStraight3Step", 0, new[] { 1 }, new[] { 5, 6, 1 }));
        Assert.IsTrue(CommandCodec.TryDecode(bytes, 1, out var decoded));
        CollectionAssert.AreEqual(new[] { 5, 6, 1 }, ((AbilityCommand)decoded).Path);
    }

    [Test]
    public void AbilityCommand_ElapsedMs_RoundTrips()
    {
        var bytes = CommandCodec.Encode(new AbilityCommand(2, "Fireball", 15, new[] { 37 }, elapsedMs: 41250));
        Assert.IsTrue(CommandCodec.TryDecode(bytes, 2, out var decoded));
        Assert.AreEqual(41250, ((AbilityCommand)decoded).ElapsedMs);
    }

    [Test]
    public void ForfeitCommand_RoundTrips()
    {
        var bytes = CommandCodec.Encode(new ForfeitCommand(2));
        Assert.IsTrue(CommandCodec.TryDecode(bytes, 2, out var decoded));
        Assert.IsInstanceOf<ForfeitCommand>(decoded);
    }

    [Test]
    public void Issuer_ComesFromReceiver_NotFromBytes()
    {
        var bytes = CommandCodec.Encode(new ForfeitCommand(1));
        Assert.IsTrue(CommandCodec.TryDecode(bytes, 2, out var decoded));
        Assert.AreEqual(2, decoded.IssuingPlayer);
    }

    [Test]
    public void SameCommand_EncodesToSameBytes()
    {
        var a = CommandCodec.Encode(new AbilityCommand(1, "Fireball", 15, new[] { 37 }));
        var b = CommandCodec.Encode(new AbilityCommand(2, "Fireball", 15, new[] { 37 }));
        CollectionAssert.AreEqual(a, b);
    }

    [Test]
    public void DecodedCommand_AppliesLikeOriginal()
    {
        var original = TestGame.Start(TestContent.Step, TestContent.Cheap);
        var replayed = TestGame.Start(TestContent.Step, TestContent.Cheap);
        var command = new AbilityCommand(1, TestContent.Step, TestGame.Player1Position, new[] { 1 });
        Assert.IsTrue(CommandCodec.TryDecode(CommandCodec.Encode(command), 1, out var decoded));

        Assert.IsTrue(original.Apply(command));
        Assert.IsTrue(replayed.Apply(decoded));
        Assert.AreEqual(original.State.PrintGrid(), replayed.State.PrintGrid());
        Assert.AreEqual(original.StateHash, replayed.StateHash);
    }

    [Test]
    public void WrongVersion_IsRejected()
    {
        var bytes = CommandCodec.Encode(new ForfeitCommand(1));
        bytes[0] = CommandCodec.Version + 1;
        Assert.IsFalse(CommandCodec.TryDecode(bytes, 1, out _));
    }

    [Test]
    public void UnknownTag_IsRejected()
    {
        var bytes = CommandCodec.Encode(new ForfeitCommand(1));
        bytes[1] = 255;
        Assert.IsFalse(CommandCodec.TryDecode(bytes, 1, out _));
    }

    [Test]
    public void EveryTruncation_IsRejectedWithoutThrowing()
    {
        var bytes = CommandCodec.Encode(new AbilityCommand(1, "Fireball", 15, new[] { 37, 38 }));
        for (var length = 0; length < bytes.Length; length++)
            Assert.IsFalse(CommandCodec.TryDecode(bytes.AsSpan(0, length).ToArray(), 1, out _), $"length {length}");
    }

    [Test]
    public void TrailingBytes_AreRejected()
    {
        var bytes = CommandCodec.Encode(new ForfeitCommand(1));
        Array.Resize(ref bytes, bytes.Length + 1);
        Assert.IsFalse(CommandCodec.TryDecode(bytes, 1, out _));
    }

    [Test]
    public void OversizedTargetCount_IsRejected()
    {
        var bytes = CommandCodec.Encode(new AbilityCommand(1, "A", 0, new int[0]));
        var countOffset = bytes.Length - 8;
        BitConverter.GetBytes(CommandCodec.MaxTargets + 1).CopyTo(bytes, countOffset);
        Assert.IsFalse(CommandCodec.TryDecode(bytes, 1, out _));
    }

    [Test]
    public void SetupCommands_HaveNoWireFormat()
    {
        Assert.Throws<NotSupportedException>(() => CommandCodec.Encode(new SpawnTeamsCommand()));
    }
}
