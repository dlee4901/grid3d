using System.Collections.Generic;
using NUnit.Framework;

[SetUpFixture]
public class TestContent
{
    public const string Cheap = "Test.Cheap";
    public const string Locked = "Test.Locked";
    public const string WarmingUp = "Test.WarmingUp";
    public const string Step = "Test.Step";
    public const string Poke = "Test.Poke";
    public const string Count = "Test.Count";
    public const string KillThenCount = "Test.KillThenCount";
    public const string CountAllies = "Test.CountAllies";
    public const string BlastAll = "Test.BlastAll";
    public const string BlastNotSelf = "Test.BlastNotSelf";
    public const string Bolt = "Test.Bolt";
    public const string Pair = "Test.Pair";
    public const string StabEnemy = "Test.StabEnemy";
    public const string Axe = "Test.Axe";
    public const string Tough = "Test.Tough";
    public const string BurnTouch = "Test.BurnTouch";
    public const string Ignite = "Test.Ignite";
    public const string Hit3 = "Test.Hit3";
    public const string PowerHit = "Test.PowerHit";
    public const string Shove = "Test.Shove";
    public const string Blazing = "Test.Blazing";
    public const string Armored = "Test.Armored";
    public const string Avenger = "Test.Avenger";
    public const string Vengeance = "Test.Vengeance";
    public const string Volatile = "Test.Volatile";
    public const string Explode = "Test.Explode";
    public const string Scavenger = "Test.Scavenger";
    public const string Feast = "Test.Feast";
    public const string Lead = "Test.Lead";
    public const string LeadKit = "Test.LeadKit";
    public const string LeadOnlyKit = "Test.LeadOnlyKit";
    public const string Lunge = "Test.Lunge";
    public const string DoubleOpenerKit = "Test.DoubleOpenerKit";
    public const string AxeThrowKit = "Test.AxeThrowKit";
    public const string GroundedKit = "Test.GroundedKit";
    public const string FlyKit = "Test.FlyKit";
    public const string RabiesKit = "Test.RabiesKit";
    public const string IncinerateKit = "Test.IncinerateKit";
    public const string SpawnCheap = "Test.SpawnCheap";
    public const string PlantThorns = "Test.PlantThorns";
    public const string ThornsKit = "Test.ThornsKit";
    public const string BlazeHitKit = "Test.BlazeHitKit";
    public const string FireMageKit = "Test.FireMageKit";
    public const string UndeadKit = "Test.UndeadKit";
    public const string CloneKit = "Test.CloneKit";
    public const string MarkKit = "Test.MarkKit";
    public const string BackstabKit = "Test.BackstabKit";
    public const string AreaHit3 = "Test.AreaHit3";
    public const string EngineerKit = "Test.EngineerKit";
    public const string NecromancerKit = "Test.NecromancerKit";
    public const string SwordSlash = "Sword Slash";
    public const string ShieldBash = "Shield Bash";

    public static readonly CountingEffect Counter = new();
    public static readonly CountingEffect AllyCounter = new() { Relation = Occupant.Ally };
    public static readonly CountingEffect NotSelfCounter = new() { Relation = Occupant.Units & ~Occupant.Self };

    public static void ResetCounters()
    {
        Counter.Reset();
        AllyCounter.Reset();
        NotSelfCounter.Reset();
    }

    public const string CostlyUnit = "Test.Unit.Costly";

    public static string UnitWith(string abilityId) => "Test.Unit." + abilityId;

    [OneTimeSetUp]
    public void Register()
    {
        Register(StatusConfigs.Burn);
        Register(TerrainConfigs.Burning);
        Register(StatusConfigs.Power);
        Register(AbilityConfigs.Armored);
        Register(AbilityConfigs.Blaze);
        Register(new AbilityConfig
        {
            Id = Vengeance,
            Trigger = new Trigger { On = GameEvent.Died },
            Effects = new List<AbilityEffect> { new DamageEffect { Relation = Occupant.Enemy, Amount = 3 } }
        });
        Register(new AbilityConfig
        {
            Id = Explode,
            Trigger = new Trigger { On = GameEvent.Died },
            Area = WithinSteps(1),
            Effects = new List<AbilityEffect> { new DamageEffect { Relation = Occupant.Units, Amount = 2 } }
        });
        Register(new AbilityConfig
        {
            Id = Feast,
            Trigger = new Trigger { On = GameEvent.Died, Subject = Occupant.Units & ~Occupant.Self, Range = WithinSteps(3) },
            Effects = new List<AbilityEffect> { new StatusEffect { Relation = Occupant.Self, Status = "Power", Amount = 1 } }
        });
        RegisterUnit(new AbilityConfig { Id = Cheap, Targeting = SelfOnly(), Effects = Idle() });
        RegisterUnit(new AbilityConfig { Id = Locked, Targeting = SelfOnly(), Effects = Idle(), Locked = true });
        RegisterUnit(new AbilityConfig { Id = WarmingUp, Targeting = SelfOnly(), Effects = Idle(), Warmup = 1 });
        RegisterUnit(new AbilityConfig
        {
            Id = Step, Targeting = AdjacentTile(),
            Effects = new List<AbilityEffect> { new MoveEffect() }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Poke, Targeting = AdjacentTile(),
            Effects = new List<AbilityEffect> { new NoOpEffect { Relation = Occupant.Any } }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Count, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = KillThenCount, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new DamageEffect { Amount = 1 }, Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = CountAllies, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { AllyCounter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = BlastAll, Targeting = AdjacentBlast(),
            Effects = new List<AbilityEffect> { Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = BlastNotSelf, Targeting = AdjacentBlast(),
            Effects = new List<AbilityEffect> { NotSelfCounter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Bolt, Targeting = StraightBolt(),
            Effects = new List<AbilityEffect> { Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Pair, Targeting = DistinctPair(),
            Effects = new List<AbilityEffect> { Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = StabEnemy, Targeting = AdjacentEnemy(),
            Effects = new List<AbilityEffect> { Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Axe, Targeting = StraightAxe(),
            Effects = new List<AbilityEffect>
            {
                new CooldownEffect { Relation = Occupant.Units & ~Occupant.Self, Turns = 2, When = Var.Distance >= 2 },
                new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = Value.If(Var.Distance <= 1, 5, 3) }
            }
        });
        RegisterUnit(new AbilityConfig { Id = Tough, Targeting = SelfOnly(), Effects = Idle() }, health: 10);
        RegisterUnit(new AbilityConfig
        {
            Id = Ignite, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect>
            {
                new TerrainEffect { Relation = Occupant.Any, Terrain = TerrainType.Burning }
            }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = BurnTouch, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new StatusEffect { Status = "Burn" } }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Hit3, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new DamageEffect { Amount = 3 } }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = PowerHit, Targeting = AdjacentBlast(),
            Effects = new List<AbilityEffect>
            {
                new StatusEffect { Relation = Occupant.Self, Status = "Power", Amount = 2 },
                new DamageEffect { Relation = Occupant.Enemy, Amount = 1 }
            }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Shove, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new PushEffect { Distance = 5 } }
        });
        RegisterUnit(AbilityConfigs.SwordSlash, health: 10);
        RegisterUnit(AbilityConfigs.ShieldBash, health: 10);
        RegisterUnit(new AbilityConfig { Id = Blazing, Targeting = SelfOnly(), Effects = Idle() }, health: 10, passives: new List<string> { "Blaze" });
        RegisterUnit(new AbilityConfig { Id = Armored, Targeting = SelfOnly(), Effects = Idle() }, health: 10, passives: new List<string> { "Armored" });
        RegisterUnit(new AbilityConfig
        {
            Id = Avenger, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new DamageEffect { Amount = 3 } }
        }, passives: new List<string> { Vengeance });
        RegisterUnit(new AbilityConfig { Id = Volatile, Targeting = SelfOnly(), Effects = Idle() }, passives: new List<string> { Explode });
        RegisterUnit(new AbilityConfig { Id = Scavenger, Targeting = SelfOnly(), Effects = Idle() }, health: 10, passives: new List<string> { Feast });
        RegisterCostlyUnit();
        RegisterKits();
    }

    private static void RegisterKits()
    {
        Register(AbilityConfigs.MoveNimble);
        Register(AbilityConfigs.MoveStraight3Step);
        Register(new AbilityConfig
        {
            Id = Lead, Keywords = Keyword.Opener, Targeting = AdjacentTile(),
            Effects = new List<AbilityEffect> { new MoveEffect() }
        });
        RegisterKit(LeadKit, Lead, Hit3);
        RegisterKit(LeadOnlyKit, Lead);
        Register(new AbilityConfig
        {
            Id = Lunge, Keywords = Keyword.Opener, Targeting = AdjacentTile(),
            Effects = new List<AbilityEffect> { new MoveEffect() }
        });
        RegisterKit(DoubleOpenerKit, Lead, Lunge, Hit3);
        Register(AbilityConfigs.AxeThrow);
        RegisterKit(AxeThrowKit, AbilityConfigs.AxeThrow.Id);
        RegisterKit(GroundedKit, AbilityConfigs.MoveStraight3Step.Id, Hit3);
        Register(AbilityConfigs.MoveFly);
        RegisterKit(FlyKit, AbilityConfigs.MoveFly.Id);
        Register(StatusConfigs.Rabies);
        Register(AbilityConfigs.RabiesBite);
        RegisterKit(RabiesKit, AbilityConfigs.RabiesBite.Id);
        Register(AbilityConfigs.Incinerate);
        RegisterKit(IncinerateKit, AbilityConfigs.Incinerate.Id);
        RegisterUnit(new AbilityConfig
        {
            Id = SpawnCheap, Targeting = AdjacentTile(),
            Effects = new List<AbilityEffect> { new SpawnEffect { Unit = UnitWith(Cheap) } }
        });
        Register(TerrainConfigs.Thorns);
        Register(new AbilityConfig
        {
            Id = PlantThorns, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new TerrainEffect { Relation = Occupant.Any, Terrain = TerrainType.Thorns, Rounds = 3 } }
        });
        RegisterKit(ThornsKit, PlantThorns, AbilityConfigs.MoveStraight3Step.Id);
        RegisterKit(BlazeHitKit, Hit3, AbilityConfigs.Blaze.Id);
        Register(AbilityConfigs.Fireball);
        RegisterKit(FireMageKit, AbilityConfigs.Fireball.Id, AbilityConfigs.Blaze.Id);
        Register(StatusConfigs.Revive);
        Register(StatusConfigs.Shield);
        Register(StatusConfigs.Marked);
        Register(AbilityConfigs.Undead);
        RegisterKit(UndeadKit, AbilityConfigs.Undead.Id, Cheap);
        Register(AbilityConfigs.Clone);
        RegisterKit(CloneKit, AbilityConfigs.Clone.Id);
        Register(AbilityConfigs.MarkForDeath);
        RegisterKit(MarkKit, AbilityConfigs.MarkForDeath.Id, Hit3);
        Register(AbilityConfigs.Backstab);
        RegisterKit(BackstabKit, AbilityConfigs.Backstab.Id);
        Register(AbilityConfigs.MoveStraight2Step);
        Register(AbilityConfigs.Slam);
        Register(AbilityConfigs.AbsorbSlime);
        Register(UnitConfigs.Slime);
        Register(AbilityConfigs.Shoot);
        Register(AbilityConfigs.UpgradeLevel);
        Register(UnitConfigs.Turret);
        Register(AbilityConfigs.UpgradeTurret);
        RegisterKit(EngineerKit, AbilityConfigs.UpgradeTurret.Id, Cheap);
        Register(AbilityConfigs.BoneBash);
        Register(UnitConfigs.Skeleton);
        Register(AbilityConfigs.SummonSkeleton);
        Register(AbilityConfigs.SoulSiphon);
        RegisterKit(NecromancerKit, AbilityConfigs.SummonSkeleton.Id, AbilityConfigs.SoulSiphon.Id);
        RegisterUnit(new AbilityConfig
        {
            Id = AreaHit3, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new DamageEffect { Amount = 3, Area = true } }
        }, health: 10);
    }

    private static void RegisterKit(string kit, params string[] abilityIds)
    {
        var unitId = UnitWith(kit);
        if (IdRegistry<EntityConfig>.TryGet(unitId, out _)) return;
        IdRegistry<EntityConfig>.Register(new EntityConfig
        {
            Id = unitId, Health = 10, Abilities = new List<string>(abilityIds)
        });
    }

    private static GridSelection WithinSteps(int steps) => new()
    {
        MinDistance = 1,
        Traversals = new List<GridTraversal>
        {
            new GridTraversal { Direction = GridDirection.Orthogonal, MaxDistance = steps, Linear = false, Passthrough = Occupant.Any }
        }
    };

    private static List<AbilityEffect> Idle() => new() { new NoOpEffect() };

    private static FillTargeting SelfOnly() => new()
    {
        EffectArea = new GridSelection
        {
            Traversals = new List<GridTraversal> { new GridTraversal { Direction = GridDirection.Orthogonal, MaxDistance = 0 } }
        }
    };

    private static DirectionTargeting StraightAxe() => new()
    {
        Grouping = new[] { 1, 0, 2, 0, 3, 0, 4, 0 },
        MaxHits = 1,
        EffectArea = new GridSelection
        {
            Traversals = new List<GridTraversal>
            {
                new GridTraversal
                {
                    Direction = GridDirection.Orthogonal, MaxDistance = 3, Linear = true, Passthrough = Occupant.Any
                }
            }
        }
    };

    private static PositionTargeting AdjacentEnemy() => new()
    {
        SelectableArea = new GridSelection
        {
            MinDistance = 1,
            Selectable = Occupant.Enemy,
            Traversals = new List<GridTraversal>
            {
                new GridTraversal
                {
                    Direction = GridDirection.Orthogonal, MaxDistance = 1, Linear = false, Passthrough = Occupant.Any
                }
            }
        }
    };

    private static DirectionTargeting StraightBolt() => new()
    {
        Grouping = new[] { 1, 0, 2, 0, 3, 0, 4, 0 },
        MaxHits = 1,
        EffectArea = new GridSelection
        {
            Traversals = new List<GridTraversal>
            {
                new GridTraversal
                {
                    Direction = GridDirection.Orthogonal, MaxDistance = 4, Linear = true, Passthrough = Occupant.Any
                }
            }
        }
    };

    private static PositionTargeting DistinctPair() => new()
    {
        Targets = 2,
        SelectableArea = new GridSelection
        {
            Selectable = Occupant.Units,
            Traversals = new List<GridTraversal>
            {
                new GridTraversal
                {
                    Direction = GridDirection.All, MaxDistance = 4, Linear = false, Passthrough = Occupant.Any
                }
            }
        }
    };

    private static void RegisterCostlyUnit()
    {
        if (IdRegistry<EntityConfig>.TryGet(CostlyUnit, out _)) return;
        IdRegistry<EntityConfig>.Register(new EntityConfig
        {
            Id = CostlyUnit, Cost = 11, Health = 1, Abilities = new List<string> { Cheap }
        });
    }

    private static PositionTargeting AdjacentBlast() => new()
    {
        SelectableArea = AdjacentTile(Occupant.Any).SelectableArea,
        EffectArea = new GridSelection
        {
            Traversals = new List<GridTraversal>
            {
                new GridTraversal
                {
                    Direction = GridDirection.Orthogonal, MaxDistance = 1, Linear = false, Passthrough = Occupant.Any
                }
            }
        }
    };

    private static PositionTargeting AdjacentTile(Occupant passthrough = Occupant.Empty) => new()
    {
        SelectableArea = new GridSelection
        {
            MinDistance = 1,
            Traversals = new List<GridTraversal>
            {
                new GridTraversal
                {
                    Direction = GridDirection.Orthogonal, MaxDistance = 1, Linear = false, Passthrough = passthrough
                }
            }
        }
    };

    private static void Register<T>(T item) where T : INameId
    {
        if (!IdRegistry<T>.TryGet(item.Id, out _)) IdRegistry<T>.Register(item);
    }

    private static void RegisterUnit(AbilityConfig ability, int health = 1, List<string> passives = null)
    {
        if (!IdRegistry<AbilityConfig>.TryGet(ability.Id, out _)) IdRegistry<AbilityConfig>.Register(ability);
        var unitId = UnitWith(ability.Id);
        if (IdRegistry<EntityConfig>.TryGet(unitId, out _)) return;
        var abilities = new List<string> { ability.Id };
        if (passives != null) abilities.AddRange(passives);
        IdRegistry<EntityConfig>.Register(new EntityConfig { Id = unitId, Health = health, Abilities = abilities });
    }
}
