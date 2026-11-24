using System.Collections.Generic;
using NUnit.Framework;

public class TerrainTests
{
    private static TurnExecutor StartOnBurning(int turns, int position = 0)
    {
        var map = new GridDefinition
        {
            Id = "Test.Map.Burning", X = 5, Y = 1, MaxTeamCost = 10,
            Terrain = new List<TerrainPlacement>
            {
                new() { TerrainType = "Burning", Turns = turns, PositionValues = new List<int> { position } }
            },
            PlayerStartPositions = new List<PositionConfig>
            {
                new() { PositionValues = new List<int> { 0 } },
                new() { PositionValues = new List<int> { 4 } }
            }
        };
        map.Init();
        var executor = TurnExecutor.ForDefinition(map);
        executor.Apply(new LoadTeamCommand(1, TestGame.Team(map, 0, TestContent.Tough)));
        executor.Apply(new LoadTeamCommand(2, TestGame.Team(map, 4, TestContent.Tough)));
        executor.Apply(new SpawnTeamsCommand());
        return executor;
    }

    [Test]
    public void Fireball_DealsDamageBeforeCasterCatchesFire()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (1, TestContent.FireMageKit) }, new[] { (4, TestContent.Tough) });
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, AbilityConfigs.Fireball.Id, 1, new[] { 4 })));
        Assert.AreEqual(7, Health(executor, 4));
        Assert.AreEqual(1, Burn(executor, 4));
    }

    [Test]
    public void Fireball_SquareOfKilledUnitStillBurns()
    {
        var executor = TestGame.StartOn(3, 3, new[] { (1, TestContent.FireMageKit) }, new[] { (4, TestContent.Cheap) });
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, AbilityConfigs.Fireball.Id, 1, new[] { 4 })));
        Assert.IsNull(executor.State.GetEntity(4));
        Assert.AreEqual(TerrainType.Burning, TerrainAt(executor, 4));
    }

    private static int Health(TurnExecutor executor, int position)
    {
        executor.State.GetEntity(position).TryGetComponent<HealthComponent>(out var health);
        return health.Current;
    }

    private static int Burn(TurnExecutor executor, int position)
    {
        executor.State.GetEntity(position).TryGetComponent<StatusComponent>(out var statuses);
        foreach (var status in statuses.Statuses) if (status.Id == "Burn") return status.Count;
        return 0;
    }

    private static TerrainType TerrainAt(TurnExecutor executor, int position)
    {
        executor.State.TryGetTerrain(new GridPosition(executor.State, position), out var terrain);
        return terrain;
    }

    private static void EndTurn(TurnExecutor executor, int player)
        => Assert.IsTrue(executor.Apply(new PassCommand(player)));

    [Test]
    public void UnitStartingOnBurning_GainsBurnImmediately_AndFireGoesOut()
    {
        var executor = StartOnBurning(6);
        Assert.AreEqual(1, Burn(executor, 0));
        Assert.AreEqual(TerrainType.Plain, TerrainAt(executor, 0));
        EndTurn(executor, 1);
        Assert.AreEqual(9, Health(executor, 0));
        Assert.AreEqual(0, Burn(executor, 0));
    }

    [Test]
    public void IgnitingOccupiedSquare_BurnsUnitImmediately()
    {
        var executor = TestGame.Start(TestContent.Ignite, TestContent.Tough, player2Position: 1);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.Ignite, TestGame.Player1Position, new[] { 1 })));
        Assert.AreEqual(10, Health(executor, 1));
        Assert.AreEqual(1, Burn(executor, 1));
        Assert.AreEqual(TerrainType.Plain, TerrainAt(executor, 1));
    }

    [Test]
    public void BurningTile_CountsDownEachTurnEnd_ThenReverts()
    {
        var executor = StartOnBurning(2, 2);
        Assert.AreEqual(TerrainType.Burning, TerrainAt(executor, 2));
        EndTurn(executor, 1);
        Assert.AreEqual(1, executor.State.GetTerrainTurns(new GridPosition(executor.State, 2)));
        EndTurn(executor, 2);
        Assert.AreEqual(TerrainType.Plain, TerrainAt(executor, 2));
    }

    [Test]
    public void TerrainEffect_IgnitesForTwoRoundsOfTurns()
    {
        var executor = TestGame.Start(TestContent.Ignite, TestContent.Tough);
        Assert.IsTrue(executor.Apply(new AbilityCommand(1, TestContent.Ignite, TestGame.Player1Position, new[] { 1 })));
        Assert.AreEqual(TerrainType.Burning, TerrainAt(executor, 1));
        Assert.AreEqual(3, executor.State.GetTerrainTurns(new GridPosition(executor.State, 1)));
        EndTurn(executor, 2);
        EndTurn(executor, 1);
        Assert.AreEqual(TerrainType.Burning, TerrainAt(executor, 1));
        EndTurn(executor, 2);
        Assert.AreEqual(TerrainType.Plain, TerrainAt(executor, 1));
    }

    [Test]
    public void UnlimitedTraversal_SelectsEveryReachableSquare()
    {
        var executor = StartOnBurning(4, 2);
        var selection = new GridSelection
        {
            Traversals = new List<GridTraversal> { new() { Direction = GridDirection.Orthogonal, Linear = false, MaxDistance = GridTraversal.Unlimited, Passthrough = Occupant.Any } }
        };
        var source = new GridSource(executor.State, new GridPosition(executor.State, 0), executor.State.GetEntity(0));
        Assert.AreEqual(5, selection.GetGridSteps(source).GetPositions().Count);
    }

    [Test]
    public void SelectionTerrainFilter_KeepsOnlyMatchingSquares()
    {
        var executor = StartOnBurning(4, 2);
        var selection = new GridSelection
        {
            Traversals = new List<GridTraversal> { new() { Direction = GridDirection.Orthogonal, Linear = false, MaxDistance = 4, Passthrough = Occupant.Any } },
            Terrain = new[] { TerrainType.Burning }
        };
        var source = new GridSource(executor.State, new GridPosition(executor.State, 0), executor.State.GetEntity(0));
        var positions = selection.GetGridSteps(source).GetPositions();
        CollectionAssert.AreEquivalent(new[] { new GridPosition(executor.State, 2) }, positions);
    }

    [Test]
    public void OnTerrainCondition_MatchesEffectSquare()
    {
        var executor = StartOnBurning(4, 2);
        var state = executor.State;
        var origin = new GridSource(state, new GridPosition(state, 0), state.GetEntity(0));
        var ctx = new EffectContext(origin, null);
        Assert.IsTrue(Is.OnTerrain(TerrainType.Burning).Eval(ctx.At(new GridPosition(state, 2), null, 2)));
        Assert.IsFalse(Is.OnTerrain(TerrainType.Burning).Eval(ctx.At(new GridPosition(state, 1), null, 1)));
    }

    [Test]
    public void BurningTile_ChangesStateHash()
    {
        var burning = StartOnBurning(2, 2).State.ComputeHash();
        var longer = StartOnBurning(3, 2).State.ComputeHash();
        Assert.AreNotEqual(burning, longer);
    }

    [Test]
    public void PlainTerrainEffect_ReportsNoTurns()
    {
        var state = TestGame.Start(TestContent.Tough, TestContent.Tough).State;
        new TerrainEffect { Terrain = TerrainType.Plain }.TryGetValueRange(state, out var plain, out _);
        new TerrainEffect { Terrain = TerrainType.Burning }.TryGetValueRange(state, out var burning, out _);
        Assert.AreEqual(0, plain);
        Assert.AreEqual(4, burning);
    }
}
