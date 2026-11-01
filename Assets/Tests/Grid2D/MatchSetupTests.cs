using System;
using System.Collections.Generic;
using NUnit.Framework;

public class MatchSetupTests
{
    private static MatchSetup ValidSetup(GridDefinition map)
        => new(map.Id, new[] { "host", "guest" }, new[]
        {
            TestGame.Team(map, TestGame.Player1Position, TestContent.Step),
            TestGame.Team(map, TestGame.Player2Position, TestContent.Cheap)
        });

    private static MatchSetup WithTeams(GridDefinition map, params TeamData[] teams)
        => new(map.Id, new[] { "host", "guest" }, teams);

    private static TeamData UnitAt(GridDefinition map, int position, string unitId)
        => new("Test.Team", map.Id, new List<(int, string)> { (position, unitId) });

    [Test]
    public void ValidSetup_PassesValidation()
    {
        var map = TestGame.Map();
        Assert.IsNull(ValidSetup(map).Validate(map));
    }

    [Test]
    public void Setup_RoundTrips()
    {
        var map = TestGame.Map();
        Assert.IsTrue(MatchSetup.TryDecode(ValidSetup(map).Encode(), out var decoded));
        Assert.AreEqual(map.Id, decoded.MapId);
        CollectionAssert.AreEqual(new[] { "host", "guest" }, decoded.PlayerIds);
        Assert.AreEqual(TestGame.Player2Position, decoded.Teams[1].UnitStartPositions[0].Position);
        Assert.AreEqual(TestContent.UnitWith(TestContent.Cheap), decoded.Teams[1].UnitStartPositions[0].UnitId);
    }

    [Test]
    public void HostAndGuest_StartFromIdenticalState()
    {
        var map = TestGame.Map();
        var hostSetup = ValidSetup(map);
        Assert.IsTrue(MatchSetup.TryDecode(hostSetup.Encode(), out var guestSetup));

        var host = TurnExecutor.ForDefinition(map);
        var guest = TurnExecutor.ForDefinition(map);
        foreach (var command in hostSetup.SeedCommands()) Assert.IsTrue(host.Apply(command));
        foreach (var command in guestSetup.SeedCommands()) Assert.IsTrue(guest.Apply(command));

        Assert.AreEqual(host.StateHash, guest.StateHash);
        Assert.IsNotNull(guest.State.GetEntity(TestGame.Player2Position));
    }

    [Test]
    public void SeatOf_FollowsPlayerOrder()
    {
        var setup = ValidSetup(TestGame.Map());
        Assert.AreEqual(1, setup.SeatOf("host"));
        Assert.AreEqual(2, setup.SeatOf("guest"));
        Assert.AreEqual(0, setup.SeatOf("stranger"));
    }

    [Test]
    public void WrongMap_IsRejected()
    {
        var map = TestGame.Map();
        var setup = new MatchSetup("Other.Map", new[] { "host", "guest" }, ValidSetup(map).Teams);
        Assert.IsNotNull(setup.Validate(map));
    }

    [Test]
    public void WrongPlayerCount_IsRejected()
    {
        var map = TestGame.Map();
        var setup = new MatchSetup(map.Id, new[] { "host" }, new[] { ValidSetup(map).Teams[0] });
        Assert.IsNotNull(setup.Validate(map));
    }

    [Test]
    public void DuplicatePlayerIds_AreRejected()
    {
        var map = TestGame.Map();
        var setup = new MatchSetup(map.Id, new[] { "host", "host" }, ValidSetup(map).Teams);
        Assert.IsNotNull(setup.Validate(map));
    }

    [Test]
    public void UnitOnOpponentSpawn_IsRejected()
    {
        var map = TestGame.Map();
        var setup = WithTeams(map,
            TestGame.Team(map, TestGame.Player2Position, TestContent.Cheap),
            TestGame.Team(map, TestGame.Player1Position, TestContent.Cheap));
        StringAssert.Contains("spawn", setup.Validate(map));
    }

    [Test]
    public void UnknownUnit_IsRejected()
    {
        var map = TestGame.Map();
        var setup = WithTeams(map,
            UnitAt(map, TestGame.Player1Position, "No.Such.Unit"),
            TestGame.Team(map, TestGame.Player2Position, TestContent.Cheap));
        StringAssert.Contains("unknown unit", setup.Validate(map));
    }

    [Test]
    public void TeamOverCostLimit_IsRejected()
    {
        var map = TestGame.Map();
        var setup = WithTeams(map,
            UnitAt(map, TestGame.Player1Position, TestContent.CostlyUnit),
            TestGame.Team(map, TestGame.Player2Position, TestContent.Cheap));
        StringAssert.Contains("cost", setup.Validate(map));
    }

    [Test]
    public void EmptyTeam_IsRejected()
    {
        var map = TestGame.Map();
        var setup = WithTeams(map,
            new TeamData("Test.Team", map.Id, new List<(int, string)>()),
            TestGame.Team(map, TestGame.Player2Position, TestContent.Cheap));
        StringAssert.Contains("no units", setup.Validate(map));
    }

    [Test]
    public void EveryTruncation_IsRejectedWithoutThrowing()
    {
        var bytes = ValidSetup(TestGame.Map()).Encode();
        for (var length = 0; length < bytes.Length; length++)
            Assert.IsFalse(MatchSetup.TryDecode(bytes.AsSpan(0, length).ToArray(), out _), $"length {length}");
    }

    [Test]
    public void TrailingBytes_AreRejected()
    {
        var bytes = ValidSetup(TestGame.Map()).Encode();
        Array.Resize(ref bytes, bytes.Length + 1);
        Assert.IsFalse(MatchSetup.TryDecode(bytes, out _));
    }

    [Test]
    public void WrongVersion_IsRejected()
    {
        var bytes = ValidSetup(TestGame.Map()).Encode();
        bytes[0] = MatchSetup.Version + 1;
        Assert.IsFalse(MatchSetup.TryDecode(bytes, out _));
    }
}
