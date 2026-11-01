using System;
using NUnit.Framework;

public class NetMessageTests
{
    private static NetMessage RoundTrip(NetMessage message)
    {
        Assert.IsTrue(NetMessage.TryDecode(message.Encode(), out var decoded));
        Assert.AreEqual(message.Type, decoded.Type);
        return decoded;
    }

    [Test]
    public void Hello_RoundTrips()
        => Assert.AreEqual("76561198000000000", RoundTrip(NetMessage.Hello("76561198000000000")).PlayerId);

    [Test]
    public void Team_RoundTrips()
    {
        var map = TestGame.Map();
        var team = RoundTrip(NetMessage.ForTeam(TestGame.Team(map, TestGame.Player2Position, TestContent.Cheap))).Team;
        Assert.AreEqual(map.Id, team.MapId);
        Assert.AreEqual(TestGame.Player2Position, team.UnitStartPositions[0].Position);
    }

    [Test]
    public void Setup_RoundTrips()
    {
        var map = TestGame.Map();
        var setup = new MatchSetup(map.Id, new[] { "host", "guest" }, new[]
        {
            TestGame.Team(map, TestGame.Player1Position, TestContent.Step),
            TestGame.Team(map, TestGame.Player2Position, TestContent.Cheap)
        });
        CollectionAssert.AreEqual(new[] { "host", "guest" }, RoundTrip(NetMessage.ForSetup(setup)).Setup.PlayerIds);
    }

    [Test]
    public void Command_RoundTrips()
    {
        var bytes = CommandCodec.Encode(new EndTurnCommand(1, 900));
        var decoded = RoundTrip(NetMessage.ForCommand(7, 0xDEADBEEFCAFEUL, bytes));
        Assert.AreEqual(7, decoded.Sequence);
        Assert.AreEqual(0xDEADBEEFCAFEUL, decoded.StateHash);
        CollectionAssert.AreEqual(bytes, decoded.CommandBytes);
    }

    [Test]
    public void EveryTruncation_IsRejectedWithoutThrowing()
    {
        var bytes = NetMessage.ForCommand(3, 42UL, CommandCodec.Encode(new EndTurnCommand(1, 0))).Encode();
        for (var length = 0; length < bytes.Length; length++)
            Assert.IsFalse(NetMessage.TryDecode(bytes.AsSpan(0, length).ToArray(), out _), $"length {length}");
    }

    [Test]
    public void OtherProtocolVersion_IsDetected()
    {
        var bytes = NetMessage.Hello("host").Encode();
        bytes[0] = NetMessage.ProtocolVersion + 1;
        Assert.IsTrue(NetMessage.IsProtocolMismatch(bytes));
        Assert.IsFalse(NetMessage.TryDecode(bytes, out _));
    }
}
