using NUnit.Framework;

public class ThreatMapTests
{
    private static GridState Start(string enemyKit)
        => TestGame.StartOn(5, 1, new[] { (0, TestContent.Tough) }, new[] { (4, enemyKit) }).State;

    private static bool Threatened(GridState state, int position)
        => ThreatMap.For(state, 1).Contains(new GridPosition(state, position));

    [Test]
    public void EnemyAttack_ThreatensItsReach()
    {
        var state = Start(TestContent.Hit3);
        Assert.IsTrue(Threatened(state, 3));
        Assert.IsFalse(Threatened(state, 2));
    }

    [Test]
    public void OwnUnits_DoNotThreatenTheirPlayer()
    {
        var state = Start(TestContent.Hit3);
        Assert.IsFalse(ThreatMap.For(state, 2).Contains(new GridPosition(state, 3)));
    }

    [Test]
    public void MoveThenAttack_BlockedWithoutKeyword_DoesNotExtendReach()
    {
        var state = Start(TestContent.GroundedKit);
        Assert.IsTrue(Threatened(state, 3));
        Assert.IsFalse(Threatened(state, 2));
    }

    [Test]
    public void OpenerMove_ThenAttack_ExtendsReach()
    {
        var state = Start(TestContent.LeadKit);
        Assert.IsTrue(Threatened(state, 2));
    }
}
