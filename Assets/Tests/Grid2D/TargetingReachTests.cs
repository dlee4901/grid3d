using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

public class TargetingReachTests
{
    private const int Center = 24;

    private static IEnumerable<AbilityConfig> CastableAbilities()
        => typeof(AbilityConfigs).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => field.GetValue(null))
            .OfType<AbilityConfig>()
            .Where(config => !config.IsPassive);

    private static GridSource Lone(out TurnExecutor executor)
    {
        executor = TestGame.StartOn(7, 7, new[] { (Center, TestContent.Tough) }, new[] { (0, TestContent.Tough) });
        var unit = executor.State.GetEntity(Center);
        return new GridSource(executor.State, unit.Position, unit);
    }

    [TestCaseSource(nameof(CastableAbilities))]
    public void CastableAbility_ShowsReach_WithNothingToHit(AbilityConfig config)
    {
        var source = Lone(out _);
        var reach = new Ability(config).Targeting.GetSelectableSteps(source).GetPositions();
        Assert.IsNotEmpty(reach, $"{config.Id} has no possible targets on an empty board, so nothing gets highlighted");
    }

    [Test]
    public void BoneBash_ReachIsAllAdjacent_ValidOnlyOnUnits()
    {
        var executor = TestGame.StartOn(7, 7, new[] { (Center, TestContent.Tough) }, new[] { (Center + 7, TestContent.Tough) });
        var unit = executor.State.GetEntity(Center);
        var source = new GridSource(executor.State, unit.Position, unit);
        var boneBash = new Ability(AbilityConfigs.BoneBash);
        var reach = boneBash.Targeting.GetSelectableSteps(source).GetPositions().Select(p => p.Dim1).OrderBy(p => p);
        CollectionAssert.AreEqual(new[] { Center - 7, Center - 1, Center + 1, Center + 7 }, reach);
        var valid = reach.Where(p => boneBash.CanAffect(source, new GridPosition(executor.State, p)));
        CollectionAssert.AreEqual(new[] { Center + 7 }, valid);
    }
}
