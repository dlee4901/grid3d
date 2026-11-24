using System.Collections.Generic;
using NUnit.Framework;

public class StatusMechanicsTests
{
    private static bool Cast(TurnExecutor executor, int player, string ability, int source, int target)
        => executor.Apply(new AbilityCommand(player, ability, source, new[] { target }));

    private static void Pass(TurnExecutor executor, int player) => Assert.IsTrue(executor.Apply(new PassCommand(player)));

    private static Entity At(TurnExecutor executor, int position) => executor.State.GetEntity(position);

    private static int Health(TurnExecutor executor, int position)
    {
        At(executor, position).TryGetComponent<HealthComponent>(out var health);
        return health.Current;
    }

    private static Status StatusAt(TurnExecutor executor, int position, StatusConfig config)
    {
        At(executor, position).TryGetComponent<StatusComponent>(out var statuses);
        return statuses.Find(config);
    }

    private static int Stacks(TurnExecutor executor, int position, StatusConfig config) => StatusAt(executor, position, config)?.Count ?? 0;

    private static void Give(TurnExecutor executor, int position, StatusConfig config, int amount, int param = 0)
        => executor.State.ApplyStatus(At(executor, position), config, amount, param);

    private static TurnExecutor StartUnits(int x, int y, (int Position, string Unit)[] player1, (int Position, string Unit)[] player2)
    {
        var map = new GridDefinition
        {
            Id = $"Test.Map.Units.{x}x{y}", X = x, Y = y, MaxTeamCost = 100,
            PlayerStartPositions = new List<PositionConfig>
            {
                new() { PositionValues = Positions(player1) },
                new() { PositionValues = Positions(player2) }
            }
        };
        map.Init();
        var executor = TurnExecutor.ForDefinition(map);
        executor.Apply(new LoadTeamCommand(1, new TeamData("Test.Team", map.Id, new List<(int, string)>(player1))));
        executor.Apply(new LoadTeamCommand(2, new TeamData("Test.Team", map.Id, new List<(int, string)>(player2))));
        executor.Apply(new SpawnTeamsCommand());
        return executor;
    }

    private static List<int> Positions((int Position, string Unit)[] units)
    {
        var positions = new List<int>();
        foreach (var unit in units) positions.Add(unit.Position);
        return positions;
    }

    [Test]
    public void Fly_IgnoresTerrainOnPathAndLanding()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.FlyKit) }, new[] { (4, TestContent.Tough) });
        executor.State.SetTerrain(new GridPosition(executor.State, 1), TerrainType.Burning, 4);
        executor.State.SetTerrain(new GridPosition(executor.State, 2), TerrainType.Burning, 4);
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.MoveFly.Id, 0, 2));
        Assert.AreEqual(10, Health(executor, 2));
    }

    [Test]
    public void Walk_TakesTerrainOnPath()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.GroundedKit) }, new[] { (4, TestContent.Tough) });
        executor.State.SetTerrain(new GridPosition(executor.State, 1), TerrainType.Burning, 4);
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.MoveStraight3Step.Id, 0, 2));
        Assert.AreEqual(9, Health(executor, 2));
    }

    [Test]
    public void Rabies_ReapplyRemovesOneStack()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.Tough) }, new[] { (1, TestContent.Tough) });
        Give(executor, 1, StatusConfigs.Rabies, 4);
        Give(executor, 1, StatusConfigs.Rabies, 4);
        Assert.AreEqual(3, Stacks(executor, 1, StatusConfigs.Rabies));
    }

    [Test]
    public void Rabies_ReapplyAtOneStack_Kills()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.Tough) }, new[] { (1, TestContent.Tough) });
        Give(executor, 1, StatusConfigs.Rabies, 1);
        Give(executor, 1, StatusConfigs.Rabies, 4);
        Assert.IsNull(At(executor, 1));
    }

    [Test]
    public void Undead_GainsReviveAtGameStart_AndRevivesFresh()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.UndeadKit) }, new[] { (4, TestContent.Tough) });
        var original = At(executor, 0);
        Assert.AreEqual(1, Stacks(executor, 0, StatusConfigs.Revive));
        Give(executor, 0, StatusConfigs.Burn, 2);
        executor.State.Kill(original, null, 0);
        executor.State.Triggers.Resolve();
        var revived = At(executor, 0);
        Assert.IsNotNull(revived);
        Assert.AreNotEqual(original.Uid, revived.Uid);
        Assert.AreEqual(0, Stacks(executor, 0, StatusConfigs.Revive));
        Assert.AreEqual(0, Stacks(executor, 0, StatusConfigs.Burn));
        Assert.AreEqual(10, Health(executor, 0));
        Assert.IsTrue(executor.State.IsControllable(revived.Position, 1));
        var order = executor.State.ResolutionOrder.Entities;
        Assert.AreSame(revived, order[order.Count - 1]);
    }

    [Test]
    public void Clone_CopiesHealthAndStatuses()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.CloneKit) }, new[] { (4, TestContent.Tough) });
        Give(executor, 0, StatusConfigs.Burn, 2);
        At(executor, 0).TryGetComponent<HealthComponent>(out var health);
        health.ApplyDamage(3);
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.Clone.Id, 0, 1));
        Assert.AreEqual(6, Health(executor, 0));
        Assert.AreEqual(6, Health(executor, 1));
        Assert.AreEqual(1, Stacks(executor, 1, StatusConfigs.Burn));
        Assert.AreNotEqual(At(executor, 0).Uid, At(executor, 1).Uid);
        Assert.IsTrue(executor.State.IsControllable(new GridPosition(executor.State, 1), 1));
    }

    [Test]
    public void AbsorbSlime_TakesStatusesAndPower_KillsSlime()
    {
        var executor = StartUnits(5, 1, new[] { (0, "Slime") }, new[] { (1, "Slime"), (4, "Slime") });
        Give(executor, 1, StatusConfigs.Power, 2);
        Give(executor, 1, StatusConfigs.Rabies, 3);
        Give(executor, 0, StatusConfigs.Rabies, 3);
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.AbsorbSlime.Id, 0, 1));
        Assert.IsNull(At(executor, 1));
        Assert.AreEqual(3, Stacks(executor, 0, StatusConfigs.Power));
        Assert.AreEqual(1, Stacks(executor, 0, StatusConfigs.Rabies));
    }

    [Test]
    public void AbsorbSlime_RejectsNonSlime()
    {
        var executor = StartUnits(5, 1, new[] { (0, "Slime") }, new[] { (1, TestContent.UnitWith(TestContent.Tough)) });
        Assert.IsFalse(Cast(executor, 1, AbilityConfigs.AbsorbSlime.Id, 0, 1));
    }

    [Test]
    public void Shield_BlocksAbilityDamageFromItsDirection_Once()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (4, TestContent.Tough) }, new[] { (5, TestContent.Hit3) });
        Give(executor, 4, StatusConfigs.Shield, 1, (int)GridDirection.East);
        Pass(executor, 1);
        Assert.IsTrue(Cast(executor, 2, TestContent.Hit3, 5, 4));
        Assert.AreEqual(10, Health(executor, 4));
        Assert.IsNull(StatusAt(executor, 4, StatusConfigs.Shield));
    }

    [Test]
    public void Shield_DoesNotBlockOtherDirections()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (4, TestContent.Tough) }, new[] { (7, TestContent.Hit3) });
        Give(executor, 4, StatusConfigs.Shield, 1, (int)GridDirection.East);
        Pass(executor, 1);
        Assert.IsTrue(Cast(executor, 2, TestContent.Hit3, 7, 4));
        Assert.AreEqual(7, Health(executor, 4));
    }

    [Test]
    public void Shield_DoesNotBlockAreaDamage()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (4, TestContent.Tough) }, new[] { (5, TestContent.AreaHit3) });
        Give(executor, 4, StatusConfigs.Shield, 1, (int)GridDirection.East);
        Pass(executor, 1);
        Assert.IsTrue(Cast(executor, 2, TestContent.AreaHit3, 5, 4));
        Assert.AreEqual(7, Health(executor, 4));
    }

    [Test]
    public void ShieldBash_GivesShieldFacingTheChosenDirection()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.ShieldBash) }, new[] { (4, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, 1, TestContent.ShieldBash, 0, 1));
        Assert.AreEqual((int)GridDirection.East, StatusAt(executor, 0, StatusConfigs.Shield).Param);
    }

    [Test]
    public void Mark_KillsOnMarkersAbilityDamage()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.MarkKit) }, new[] { (1, TestContent.Tough), (4, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.MarkForDeath.Id, 0, 1));
        Pass(executor, 2);
        Assert.IsTrue(Cast(executor, 1, TestContent.Hit3, 0, 1));
        Assert.IsNull(At(executor, 1));
    }

    [Test]
    public void Mark_OnlyOneTargetPerMarker()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.MarkKit) }, new[] { (1, TestContent.Tough), (4, TestContent.Tough) });
        var marker = At(executor, 0).Uid;
        Give(executor, 1, StatusConfigs.Marked, 1, marker);
        Give(executor, 4, StatusConfigs.Marked, 1, marker);
        Assert.IsNull(StatusAt(executor, 1, StatusConfigs.Marked));
        Assert.IsNotNull(StatusAt(executor, 4, StatusConfigs.Marked));
    }

    [Test]
    public void Mark_RemovedWhenMarkerDies()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.MarkKit), (2, TestContent.Tough) }, new[] { (4, TestContent.Tough) });
        var marker = At(executor, 0);
        Give(executor, 4, StatusConfigs.Marked, 1, marker.Uid);
        executor.State.Kill(marker, null, 0);
        executor.State.Triggers.Resolve();
        Assert.IsNull(StatusAt(executor, 4, StatusConfigs.Marked));
    }

    [Test]
    public void PlayerTwo_FacesSouth()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.Tough) }, new[] { (4, TestContent.Tough) });
        Assert.AreEqual(GridDirectionFacing.North, At(executor, 0).Facing);
        Assert.AreEqual(GridDirectionFacing.South, At(executor, 4).Facing);
    }

    [Test]
    public void Backstab_FromFront_Damages()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (1, TestContent.BackstabKit) }, new[] { (4, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.Backstab.Id, 1, 4));
        Assert.AreEqual(6, Health(executor, 4));
    }

    [Test]
    public void Backstab_FromBehind_Kills()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (7, TestContent.BackstabKit) }, new[] { (4, TestContent.Tough), (0, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.Backstab.Id, 7, 4));
        Assert.IsNull(At(executor, 4));
    }

    [Test]
    public void PlayerTwo_Incinerate_HitsTheChosenDirection()
    {
        var executor = TestGame.StartOn(5, 5, new[] { (7, TestContent.Tough), (1, TestContent.Tough) }, new[] { (12, TestContent.IncinerateKit) });
        Pass(executor, 1);
        Assert.IsTrue(Cast(executor, 2, AbilityConfigs.Incinerate.Id, 12, 7));
        Assert.AreEqual(6, Health(executor, 7));
        Assert.AreEqual(7, Health(executor, 1));
    }

    [Test]
    public void PlayerTwo_CrossbowRay_ReachesFullRange()
    {
        TestContent.ResetCounters();
        var executor = TestGame.StartOn(1, 6, new[] { (1, TestContent.Tough) }, new[] { (5, TestContent.Bolt) });
        Pass(executor, 1);
        Assert.IsTrue(Cast(executor, 2, TestContent.Bolt, 5, 4));
        Assert.AreEqual(1, TestContent.Counter.Hits);
    }

    [Test]
    public void PlayerTwo_SwordSlash_HitsTheChosenDirection()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (0, TestContent.Tough), (1, TestContent.Tough) }, new[] { (4, TestContent.SwordSlash) });
        Pass(executor, 1);
        Assert.IsTrue(Cast(executor, 2, TestContent.SwordSlash, 4, 1));
        Assert.AreEqual(6, Health(executor, 0));
        Assert.AreEqual(6, Health(executor, 1));
    }

    private static void SetLevel(TurnExecutor executor, int position, int level)
    {
        At(executor, position).TryGetComponent<AbilityComponent>(out var abilities);
        var counter = abilities.Dictionary[AbilityConfigs.UpgradeLevel.Id];
        counter.AdjustCount(level - counter.Count, 3);
    }

    private static string Kit(string kit) => TestContent.UnitWith(kit);

    [Test]
    public void Turret_ShootsFirstUnitAhead_EveryOtherOwnTurn()
    {
        var executor = StartUnits(2, 6, new[] { (0, "Turret"), (1, Kit(TestContent.Tough)) }, new[] { (4, Kit(TestContent.Tough)), (6, Kit(TestContent.Tough)) });
        Pass(executor, 1);
        Assert.AreEqual(7, Health(executor, 4));
        Assert.AreEqual(10, Health(executor, 6));
        Pass(executor, 2);
        Pass(executor, 1);
        Assert.AreEqual(7, Health(executor, 4));
        Pass(executor, 2);
        Pass(executor, 1);
        Assert.AreEqual(4, Health(executor, 4));
    }

    [Test]
    public void Turret_PlayerTwo_ShootsSouth()
    {
        var executor = StartUnits(2, 6, new[] { (6, Kit(TestContent.Tough)) }, new[] { (10, "Turret"), (11, Kit(TestContent.Tough)) });
        Pass(executor, 1);
        Pass(executor, 2);
        Assert.AreEqual(7, Health(executor, 6));
    }

    [Test]
    public void Turret_Upgrades_AddRangeThenDamageThenPierce()
    {
        var executor = StartUnits(2, 6, new[] { (0, "Turret"), (1, Kit(TestContent.Tough)) }, new[] { (8, Kit(TestContent.Tough)), (9, Kit(TestContent.Tough)) });
        Pass(executor, 1);
        Assert.AreEqual(10, Health(executor, 8));
        SetLevel(executor, 0, 1);
        Pass(executor, 2);
        Pass(executor, 1);
        Assert.AreEqual(7, Health(executor, 8));
        SetLevel(executor, 0, 2);
        Pass(executor, 2);
        Pass(executor, 1);
        Pass(executor, 2);
        Pass(executor, 1);
        Assert.AreEqual(3, Health(executor, 8));
    }

    [Test]
    public void Turret_Pierce_HitsEveryUnitInRange()
    {
        var executor = StartUnits(2, 6, new[] { (0, "Turret"), (1, Kit(TestContent.Tough)) }, new[] { (4, Kit(TestContent.Tough)), (6, Kit(TestContent.Tough)), (9, Kit(TestContent.Tough)) });
        SetLevel(executor, 0, 3);
        Pass(executor, 1);
        Assert.AreEqual(6, Health(executor, 4));
        Assert.AreEqual(6, Health(executor, 6));
    }

    [Test]
    public void UpgradeTurret_RaisesLevel_UpToThree_AnyTurretOnly()
    {
        var executor = StartUnits(5, 1, new[] { (1, Kit(TestContent.EngineerKit)), (0, "Turret") }, new[] { (2, Kit(TestContent.Tough)), (4, "Turret") });
        Assert.IsFalse(Cast(executor, 1, AbilityConfigs.UpgradeTurret.Id, 1, 2));
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.UpgradeTurret.Id, 1, 0));
        At(executor, 0).TryGetComponent<AbilityComponent>(out var abilities);
        Assert.AreEqual(1, abilities.Dictionary[AbilityConfigs.UpgradeLevel.Id].Count);
        SetLevel(executor, 0, 3);
        Pass(executor, 2);
        Assert.IsFalse(Cast(executor, 1, AbilityConfigs.UpgradeTurret.Id, 1, 0));
    }

    [Test]
    public void SummonSkeleton_SpawnsSkeletonWithRevive()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.NecromancerKit) }, new[] { (4, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.SummonSkeleton.Id, 0, 1));
        Assert.AreEqual("Skeleton", At(executor, 1).Id);
        Assert.AreEqual(1, Stacks(executor, 1, StatusConfigs.Revive));
    }

    [Test]
    public void SoulSiphon_CountsDeaths_AndHitsKiller()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.NecromancerKit), (1, TestContent.Tough) }, new[] { (3, TestContent.Tough), (4, TestContent.Tough) });
        executor.State.Kill(At(executor, 1), null, 0);
        executor.State.Kill(At(executor, 4), null, 0);
        executor.State.Triggers.Resolve();
        At(executor, 0).TryGetComponent<AbilityComponent>(out var abilities);
        Assert.AreEqual(2, abilities.Dictionary[AbilityConfigs.SoulSiphon.Id].Count);
        executor.State.Kill(At(executor, 0), At(executor, 3), 3);
        executor.State.Triggers.Resolve();
        Assert.AreEqual(8, Health(executor, 3));
    }
}
