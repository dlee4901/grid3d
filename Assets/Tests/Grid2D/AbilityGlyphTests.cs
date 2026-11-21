using System.Collections.Generic;
using NUnit.Framework;

public class AbilityGlyphTests
{
    private const int North = 1 << 0, NorthEast = 1 << 1, East = 1 << 2, SouthEast = 1 << 3,
                      South = 1 << 4, SouthWest = 1 << 5, West = 1 << 6, NorthWest = 1 << 7;
    private const int Cardinal = North | East | South | West;
    private const int Diagonals = NorthEast | SouthEast | SouthWest | NorthWest;

    private static List<(int X, int Y)> Area(AbilityConfig config, out bool fromCaster)
    {
        Assert.IsTrue(TargetingShape.TryGetArea(config.Targeting, out var cells, out fromCaster));
        return cells;
    }

    [Test]
    public void DirectionMask_ReflectsTraversalDirections()
    {
        Assert.AreEqual(Cardinal, TargetingShape.DirectionMask(AbilityConfigs.MoveStraight3Step.Targeting));
        Assert.AreEqual(Cardinal, TargetingShape.DirectionMask(AbilityConfigs.SwordSlash.Targeting));
        Assert.AreEqual(Diagonals, TargetingShape.DirectionMask(AbilityConfigs.CrossbowShot.Targeting));
        Assert.AreEqual(Cardinal | Diagonals, TargetingShape.DirectionMask(AbilityConfigs.AxeSwing.Targeting));
    }

    [Test]
    public void SwordSlash_AreaIsNorthCone()
    {
        var cells = Area(AbilityConfigs.SwordSlash, out var fromCaster);
        Assert.IsTrue(fromCaster);
        CollectionAssert.AreEquivalent(new[] { (-1, 1), (0, 1), (1, 1) }, cells);
    }

    [Test]
    public void AxeSwing_AreaIsNorthRay()
    {
        var cells = Area(AbilityConfigs.AxeSwing, out _);
        CollectionAssert.AreEquivalent(new[] { (0, 1), (0, 2), (0, 3) }, cells);
    }

    [Test]
    public void CrossbowShot_AreaIsFirstDiagonalRay()
    {
        var cells = Area(AbilityConfigs.CrossbowShot, out _);
        CollectionAssert.AreEquivalent(new[] { (1, 1), (2, 2), (3, 3), (4, 4) }, cells);
    }

    [Test]
    public void Fireball_AreaIsBlastAroundTarget()
    {
        var cells = Area(AbilityConfigs.Fireball, out var fromCaster);
        Assert.IsFalse(fromCaster);
        Assert.AreEqual(9, cells.Count);
    }

    [Test]
    public void ReachesAnywhere_OnlyForTeleportsAndUnlimitedRange()
    {
        Assert.IsTrue(TargetingShape.ReachesAnywhere(new Ability(AbilityConfigs.FlameStep)));
        Assert.IsFalse(TargetingShape.ReachesAnywhere(new Ability(AbilityConfigs.MoveStraight3Step)));
        Assert.IsFalse(TargetingShape.ReachesAnywhere(new Ability(AbilityConfigs.Fireball)));
        Assert.IsFalse(TargetingShape.ReachesAnywhere(new Ability(AbilityConfigs.CrossbowShot)));
    }

    [Test]
    public void SingleTargetAbility_HasNoArea()
    {
        Assert.IsFalse(TargetingShape.TryGetArea(AbilityConfigs.ShieldBash.Targeting, out _, out _));
    }
}
