using System;
using System.Collections.Generic;
using NUnit.Framework;

public class ShapeTests
{
    private static readonly string[] Diamond = { "..#..", ".###.", "#####", ".###.", "..#.." };

    private static TurnExecutor Start(string ability = TestContent.Step)
        => TestGame.StartOn(5, 5, new[] { (6, ability) }, new[] { (22, TestContent.Tough) }, layout: Diamond);

    [Test]
    public void Holes_AreInvalid_TilesAreValid()
    {
        var state = Start().State;
        foreach (var hole in new[] { 0, 1, 3, 4, 5, 9, 15, 19, 20, 21, 23, 24 })
            Assert.IsFalse(new GridPosition(state, hole).IsValid(), $"hole {hole}");
        foreach (var tile in new[] { 2, 6, 7, 8, 10, 11, 12, 13, 14, 16, 17, 18, 22 })
            Assert.IsTrue(new GridPosition(state, tile).IsValid(), $"tile {tile}");
    }

    [Test]
    public void Add_IntoHole_IsInvalid()
    {
        var state = Start().State;
        Assert.IsFalse(new GridPosition(state, 6).Add((-1, 0)).IsValid());
        Assert.IsTrue(new GridPosition(state, 6).Add((1, 0)).IsValid());
    }

    [Test]
    public void Step_IntoHole_IsRejected()
    {
        var executor = Start();
        Assert.IsFalse(executor.Apply(new AbilityCommand(1, TestContent.Step, 6, new[] { 5 })));
        Assert.IsFalse(executor.Apply(new AbilityCommand(1, TestContent.Step, 6, new[] { 1 })));
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.Step, 6, new[] { 7 })));
    }

    [Test]
    public void Spawns_OnHoles_AreDropped()
    {
        var map = new GridDefinition
        {
            Id = "Test.Diamond", X = 5, Y = 5, Layout = Diamond,
            PlayerStartPositions = new List<PositionConfig> { new() { PositionValues = new List<int> { 0, 2, 4 } } }
        };
        map.Init();
        CollectionAssert.AreEquivalent(new[] { 2 }, map.GetPlayerSpawnsOrNull(1));
    }

    [Test]
    public void ThreatMap_NeverIncludesHoles()
    {
        var state = TestGame.StartOn(5, 5, new[] { (12, TestContent.Tough) }, new[] { (6, TestContent.Hit3) }, layout: Diamond).State;
        foreach (var position in ThreatMap.For(state, 1)) Assert.IsTrue(position.IsValid());
    }

    [Test]
    public void Layout_WithWrongSize_Throws()
    {
        var map = new GridDefinition { Id = "Test.Bad", X = 5, Y = 2, Layout = new[] { "#####", "###" } };
        Assert.Throws<ArgumentException>(() => map.Init());
    }

    [Test]
    public void NoLayout_IsFullRectangle()
    {
        var state = TestGame.StartOn(3, 2, new[] { (0, TestContent.Tough) }, new[] { (5, TestContent.Tough) }).State;
        for (var i = 0; i < 6; i++) Assert.IsTrue(new GridPosition(state, i).IsValid());
    }
}
