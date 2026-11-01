using System.Linq;
using NUnit.Framework;

public class LockstepSessionTests
{
    private sealed class Peer
    {
        public LoopbackLink Link;
        public LockstepSession Session;
        public TurnExecutor Executor;
    }

    private static readonly GridDefinition Map = TestGame.Map();

    private static GridDefinition FindMap(string id) => id == Map.Id ? Map : null;

    private static Peer CreatePeer(LoopbackLink link, string playerId, bool isHost, TeamData team)
    {
        var peer = new Peer { Link = link };
        peer.Session = new LockstepSession(link, playerId, isHost, isHost ? Map.Id : null, FindMap, team);
        peer.Session.MatchStarted += setup =>
        {
            peer.Executor = TurnExecutor.ForDefinition(FindMap(setup.MapId));
            foreach (var command in setup.SeedCommands()) peer.Executor.Apply(command);
            peer.Session.Attach(peer.Executor);
        };
        return peer;
    }

    private static (Peer host, Peer guest) Connect(TeamData guestTeam = null)
    {
        var (hostLink, guestLink) = LoopbackLink.CreatePair();
        var host = CreatePeer(hostLink, "host", true, TestGame.Team(Map, TestGame.Player1Position, TestContent.Step));
        var guest = CreatePeer(guestLink, "guest", false,
                               guestTeam ?? TestGame.Team(Map, TestGame.Player2Position, TestContent.Cheap));
        host.Session.Start();
        guest.Session.Start();
        Pump(host.Link, guest.Link);
        return (host, guest);
    }

    private static void Pump(params LoopbackLink[] links)
    {
        while (links.Any(link => link.HasPending))
            foreach (var link in links) link.DeliverPending();
    }

    private static void SendRaw(LoopbackLink from, NetMessage message) => from.Send(message.Encode());

    [Test]
    public void Handshake_StartsBothPeersFromTheSameState()
    {
        var (host, guest) = Connect();
        Assert.AreEqual(LockstepSession.SessionPhase.Playing, host.Session.Phase);
        Assert.AreEqual(LockstepSession.SessionPhase.Playing, guest.Session.Phase);
        Assert.AreEqual(1, host.Session.LocalSeat);
        Assert.AreEqual(2, guest.Session.LocalSeat);
        Assert.AreEqual(host.Executor.StateHash, guest.Executor.StateHash);
    }

    [Test]
    public void HostMove_IsMirroredOnGuest()
    {
        var (host, guest) = Connect();
        Assert.IsTrue(host.Executor.Apply(new AbilityCommand(1, TestContent.Step, TestGame.Player1Position, new[] { 1 })));
        Pump(host.Link, guest.Link);
        Assert.IsNotNull(guest.Executor.State.GetEntity(1));
        Assert.AreEqual(host.Executor.StateHash, guest.Executor.StateHash);
    }

    [Test]
    public void TurnHandoff_StaysInSync()
    {
        var (host, guest) = Connect();
        Assert.IsTrue(host.Executor.Apply(new EndTurnCommand(1, 1200)));
        Pump(host.Link, guest.Link);
        Assert.IsTrue(guest.Executor.Apply(new EndTurnCommand(2, 3400)));
        Pump(host.Link, guest.Link);
        Assert.AreEqual(1, guest.Executor.State.ActivePlayer);
        Assert.AreEqual(host.Executor.StateHash, guest.Executor.StateHash);
        Assert.AreEqual(LockstepSession.SessionPhase.Playing, host.Session.Phase);
        Assert.AreEqual(LockstepSession.SessionPhase.Playing, guest.Session.Phase);
    }

    [Test]
    public void InvalidGuestTeam_FailsHostBeforeMatchStarts()
    {
        var (host, guest) = Connect(TestGame.Team(Map, TestGame.Player1Position, TestContent.Cheap));
        StringAssert.Contains("invalid setup", host.Session.FailureReason);
        Assert.AreEqual(LockstepSession.SessionPhase.Handshake, guest.Session.Phase);
    }

    [Test]
    public void OffTurnPeerCommand_IsRejected()
    {
        var (host, guest) = Connect();
        var sequence = host.Executor.Log.Count;
        SendRaw(guest.Link, NetMessage.ForCommand(sequence, 0UL, CommandCodec.Encode(new EndTurnCommand(2))));
        Pump(host.Link, guest.Link);
        StringAssert.Contains("rejected", host.Session.FailureReason);
    }

    [Test]
    public void WrongStateHash_IsReportedAsDesync()
    {
        var (host, guest) = Connect();
        var sequence = guest.Executor.Log.Count;
        SendRaw(host.Link, NetMessage.ForCommand(sequence, 12345UL, CommandCodec.Encode(new EndTurnCommand(1))));
        Pump(host.Link, guest.Link);
        StringAssert.Contains("desync", guest.Session.FailureReason);
    }

    [Test]
    public void SkippedSequence_IsRejected()
    {
        var (host, guest) = Connect();
        var sequence = guest.Executor.Log.Count + 1;
        SendRaw(host.Link, NetMessage.ForCommand(sequence, 0UL, CommandCodec.Encode(new EndTurnCommand(1))));
        Pump(host.Link, guest.Link);
        StringAssert.Contains("expected command", guest.Session.FailureReason);
    }

    [Test]
    public void OtherProtocolVersion_FailsSession()
    {
        var (host, guest) = Connect();
        var bytes = NetMessage.Hello("guest").Encode();
        bytes[0] = NetMessage.ProtocolVersion + 1;
        guest.Link.Send(bytes);
        Pump(host.Link, guest.Link);
        StringAssert.Contains("protocol", host.Session.FailureReason);
    }

    [Test]
    public void HostChangingGuestTeam_FailsGuest()
    {
        var (fakeHostLink, guestLink) = LoopbackLink.CreatePair();
        var guest = CreatePeer(guestLink, "guest", false, TestGame.Team(Map, TestGame.Player2Position, TestContent.Cheap));
        guest.Session.Start();
        SendRaw(fakeHostLink, NetMessage.Hello("host"));
        SendRaw(fakeHostLink, NetMessage.ForSetup(new MatchSetup(Map.Id, new[] { "host", "guest" }, new[]
        {
            TestGame.Team(Map, TestGame.Player1Position, TestContent.Step),
            TestGame.Team(Map, TestGame.Player2Position, TestContent.Step)
        })));
        Pump(fakeHostLink, guestLink);
        StringAssert.Contains("changed this player's team", guest.Session.FailureReason);
    }

    [Test]
    public void OpponentDisconnect_AwardsTheWinLocally()
    {
        var (host, guest) = Connect();
        host.Session.OpponentDisconnected("left the lobby");
        Assert.AreEqual(GameOutcome.Victory, host.Executor.State.Outcome);
        Assert.AreEqual(1, host.Executor.State.Winner);
        Assert.IsFalse(guest.Link.HasPending);
    }

    [Test]
    public void RejectedPeerCommand_ForfeitsThePeer()
    {
        var (host, guest) = Connect();
        SendRaw(guest.Link, NetMessage.ForCommand(host.Executor.Log.Count, 0UL, CommandCodec.Encode(new EndTurnCommand(2))));
        Pump(host.Link, guest.Link);
        Assert.AreEqual(1, host.Executor.State.Winner);
    }

    [Test]
    public void Desync_StopsTheMatchWithoutAWinner()
    {
        var (host, guest) = Connect();
        SendRaw(host.Link, NetMessage.ForCommand(guest.Executor.Log.Count, 12345UL, CommandCodec.Encode(new EndTurnCommand(1))));
        Pump(host.Link, guest.Link);
        Assert.AreEqual(LockstepSession.SessionPhase.Failed, guest.Session.Phase);
        Assert.AreEqual(GameOutcome.InProgress, guest.Executor.State.Outcome);
    }

    [Test]
    public void DisconnectBeforeMatch_DoesNotThrow()
    {
        var (hostLink, _) = LoopbackLink.CreatePair();
        var host = CreatePeer(hostLink, "host", true, TestGame.Team(Map, TestGame.Player1Position, TestContent.Step));
        host.Session.Start();
        host.Session.OpponentDisconnected("never joined");
        StringAssert.Contains("disconnected", host.Session.FailureReason);
    }
}
