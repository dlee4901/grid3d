using NUnit.Framework;

public class NewMechanicsTests
{
    private static bool Cast(TurnExecutor executor, int player, string ability, int source, int target)
        => executor.Apply(new AbilityCommand(player, ability, source, new[] { target }));

    private static void Pass(TurnExecutor executor, int player) => Assert.IsTrue(executor.Apply(new PassCommand(player)));

    private static int Health(TurnExecutor executor, int position)
    {
        executor.State.GetEntity(position).TryGetComponent<HealthComponent>(out var health);
        return health.Current;
    }

    [Test]
    public void Fly_PassesOverUnit()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.FlyKit) }, new[] { (1, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.MoveFly.Id, 0, 2));
        Assert.IsNull(executor.State.GetEntity(0));
        Assert.IsNotNull(executor.State.GetEntity(1));
        Assert.IsNotNull(executor.State.GetEntity(2));
    }

    [Test]
    public void Fly_CannotLandOnUnit()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.FlyKit) }, new[] { (1, TestContent.Tough) });
        Assert.IsFalse(Cast(executor, 1, AbilityConfigs.MoveFly.Id, 0, 1));
    }

    [Test]
    public void Walk_CannotPassUnit()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.GroundedKit) }, new[] { (1, TestContent.Tough) });
        Assert.IsFalse(Cast(executor, 1, AbilityConfigs.MoveStraight3Step.Id, 0, 2));
    }

    [Test]
    public void Rabies_KillsAtEndOfHoldersFourthTurn()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.RabiesKit) }, new[] { (1, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.RabiesBite.Id, 0, 1));
        for (var turn = 1; turn <= 3; turn++)
        {
            Pass(executor, 2);
            Assert.IsNotNull(executor.State.GetEntity(1), $"alive after turn {turn}");
            Pass(executor, 1);
        }
        Pass(executor, 2);
        Assert.IsNull(executor.State.GetEntity(1));
    }

    [Test]
    public void Spawn_CreatesUnitOwnedByCaster()
    {
        var executor = TestGame.Start(TestContent.SpawnCheap, TestContent.Cheap);
        Assert.IsTrue(Cast(executor, 1, TestContent.SpawnCheap, TestGame.Player1Position, 1));
        var spawned = executor.State.GetEntity(1);
        Assert.IsNotNull(spawned);
        Assert.IsTrue(executor.State.IsControllable(spawned.Position, 1));
        Assert.IsTrue(executor.State.ResolutionOrder.Contains(spawned));
    }

    [Test]
    public void Incinerate_DamageFallsWithDistance()
    {
        var executor = TestGame.StartOn(5, 5, new[] { (12, TestContent.IncinerateKit) }, new[] { (17, TestContent.Tough), (21, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.Incinerate.Id, 12, 17));
        Assert.AreEqual(6, Health(executor, 17));
        Assert.AreEqual(7, Health(executor, 21));
    }

    [Test]
    public void Thorns_PlacedUnderUnit_DoesNotDamage()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.ThornsKit) }, new[] { (1, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, 1, TestContent.PlantThorns, 0, 1));
        Assert.AreEqual(10, Health(executor, 1));
    }

    [Test]
    public void Thorns_MovingOnto_Damages()
    {
        var executor = TestGame.StartOn(5, 1, new[] { (0, TestContent.ThornsKit) }, new[] { (4, TestContent.Tough) });
        Assert.IsTrue(Cast(executor, 1, TestContent.PlantThorns, 0, 1));
        Pass(executor, 2);
        Assert.IsTrue(Cast(executor, 1, AbilityConfigs.MoveStraight3Step.Id, 0, 1));
        Assert.AreEqual(9, Health(executor, 1));
    }
}
