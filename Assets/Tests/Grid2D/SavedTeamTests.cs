using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;

public class SavedTeamTests
{
    private static GridDefinition Map(string id, int[] side1, int[] side2, string[] layout = null)
    {
        var map = new GridDefinition
        {
            Id = id, X = 4, Y = 4, Layout = layout, MaxTeamCost = 100,
            PlayerStartPositions = new List<PositionConfig>
            {
                new() { PositionValues = new List<int>(side1) },
                new() { PositionValues = new List<int>(side2) }
            }
        };
        map.Init();
        return map;
    }

    private static GridDefinition Mirrored() => Map("Test.Mirrored", new[] { 0, 1, 2, 3 }, new[] { 12, 13, 14, 15 });

    private static SavedTeam Team(GridDefinition map, params (string Unit, int Side1, int Side2)[] units)
    {
        var list = new List<SavedUnit>();
        foreach (var unit in units) list.Add(new SavedUnit(unit.Unit, new[] { unit.Side1, unit.Side2 }));
        return new SavedTeam("Test.Saved", map.Id, list);
    }

    private static string Unit => TestContent.UnitWith(TestContent.Cheap);

    [Test]
    public void PointSymmetricMap_IsMirrored()
    {
        var map = Mirrored();
        Assert.IsTrue(map.IsMirrored);
        Assert.AreEqual(15, map.Mirror(0));
        Assert.AreEqual(13, map.Mirror(2));
    }

    [Test]
    public void AsymmetricSpawns_AreNotMirrored()
        => Assert.IsFalse(Map("Test.Lopsided", new[] { 0, 1 }, new[] { 12, 13, 14, 15 }).IsMirrored);

    [Test]
    public void AsymmetricLayout_IsNotMirrored()
        => Assert.IsFalse(Map("Test.Holey", new[] { 0, 1, 2, 3 }, new[] { 12, 13, 14, 15 },
            new[] { "####", "#.##", "####", "####" }).IsMirrored);

    [Test]
    public void ForSeat_UsesThatSidesSquares_InTeamOrder()
    {
        var map = Mirrored();
        var team = Team(map, (Unit, 2, 13), (Unit, 0, 15));
        var seat2 = team.ForSeat(2);
        Assert.AreEqual(13, seat2.UnitStartPositions[0].Position);
        Assert.AreEqual(15, seat2.UnitStartPositions[1].Position);
        Assert.AreEqual(0, team.ForSeat(1).UnitStartPositions[1].Position);
    }

    [Test]
    public void Validate_AcceptsCompleteTeam()
        => Assert.IsNull(Team(Mirrored(), (Unit, 2, 13), (Unit, 0, 15)).Validate(Mirrored()));

    [Test]
    public void Validate_RejectsUnplacedSide()
        => StringAssert.Contains("no side 2 square", Team(Mirrored(), (Unit, 2, SavedUnit.Unplaced)).Validate(Mirrored()));

    [Test]
    public void Validate_RejectsSquareOutsideThatSidesSpawn()
        => StringAssert.Contains("side 2", Team(Mirrored(), (Unit, 2, 3)).Validate(Mirrored()));

    [Test]
    public void Validate_RejectsOtherMap()
        => StringAssert.Contains("for map", Team(Mirrored(), (Unit, 2, 13)).Validate(Map("Test.Other", new[] { 0 }, new[] { 15 })));

    [Test]
    public void Json_RoundTrips()
    {
        var team = Team(Mirrored(), (Unit, 2, 13), (Unit, 0, 15));
        var copy = JsonConvert.DeserializeObject<SavedTeam>(JsonConvert.SerializeObject(team));
        Assert.AreEqual(team.Id, copy.Id);
        Assert.AreEqual(team.MapId, copy.MapId);
        Assert.AreEqual(2, copy.Units.Count);
        Assert.AreEqual(Unit, copy.Units[1].UnitId);
        Assert.AreEqual(15, copy.Units[1].PositionFor(2));
    }
}
