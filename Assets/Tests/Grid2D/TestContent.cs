using System.Collections.Generic;
using NUnit.Framework;

[SetUpFixture]
public class TestContent
{
    public const string Cheap = "Test.Cheap";
    public const string Expensive = "Test.Expensive";
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
    public const string Retaliator = "Test.Retaliator";
    public const string Avenger = "Test.Avenger";
    public const string Vengeance = "Test.Vengeance";
    public const string Volatile = "Test.Volatile";
    public const string Explode = "Test.Explode";
    public const string Scavenger = "Test.Scavenger";
    public const string Feast = "Test.Feast";
    public const string Dash = "Test.Dash";
    public const string Lead = "Test.Lead";
    public const string LeadKit = "Test.LeadKit";
    public const string FlameStepKit = "Test.FlameStepKit";
    public const string AxeSwingKit = "Test.AxeSwingKit";
    public const string Double = "Test.Double";
    public const string DashKit = "Test.DashKit";
    public const string DoubleKit = "Test.DoubleKit";
    public const string NimbleKit = "Test.NimbleKit";
    public const string GroundedKit = "Test.GroundedKit";
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
        Register(AbilityConfigs.Retaliate);
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
        RegisterUnit(new AbilityConfig { Id = Cheap, ManaCost = 1 });
        RegisterUnit(new AbilityConfig { Id = Expensive, ManaCost = 5 });
        RegisterUnit(new AbilityConfig { Id = Locked, ManaCost = 1, Locked = true });
        RegisterUnit(new AbilityConfig { Id = WarmingUp, ManaCost = 1, Warmup = 1 });
        RegisterUnit(new AbilityConfig
        {
            Id = Step, ManaCost = 1, Targeting = AdjacentTile(),
            Effects = new List<AbilityEffect> { new MoveEffect() }
        });
        RegisterUnit(new AbilityConfig { Id = Poke, ManaCost = 1, Targeting = AdjacentTile() });
        RegisterUnit(new AbilityConfig
        {
            Id = Count, ManaCost = 1, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = KillThenCount, ManaCost = 1, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new DamageEffect { Amount = 1 }, Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = CountAllies, ManaCost = 1, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { AllyCounter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = BlastAll, ManaCost = 1, Targeting = AdjacentBlast(),
            Effects = new List<AbilityEffect> { Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = BlastNotSelf, ManaCost = 1, Targeting = AdjacentBlast(),
            Effects = new List<AbilityEffect> { NotSelfCounter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Bolt, ManaCost = 1, Targeting = StraightBolt(),
            Effects = new List<AbilityEffect> { Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Pair, ManaCost = 1, Targeting = DistinctPair(),
            Effects = new List<AbilityEffect> { Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = StabEnemy, ManaCost = 1, Targeting = AdjacentEnemy(),
            Effects = new List<AbilityEffect> { Counter }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Axe, ManaCost = 1, Targeting = StraightAxe(),
            Effects = new List<AbilityEffect>
            {
                new CooldownEffect { Relation = Occupant.Units & ~Occupant.Self, Turns = 2, When = Var.Distance >= 2 },
                new DamageEffect { Relation = Occupant.Units & ~Occupant.Self, Amount = Value.If(Var.Distance <= 1, 5, 3) }
            }
        });
        RegisterUnit(new AbilityConfig { Id = Tough, ManaCost = 1 }, health: 10);
        RegisterUnit(new AbilityConfig
        {
            Id = Ignite, ManaCost = 1, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect>
            {
                new TerrainEffect { Relation = Occupant.Any, Terrain = TerrainType.Burning }
            }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = BurnTouch, ManaCost = 1, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new StatusEffect { Status = "Burn" } }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Hit3, ManaCost = 1, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new DamageEffect { Amount = 3 } }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = PowerHit, ManaCost = 1, Targeting = AdjacentBlast(),
            Effects = new List<AbilityEffect>
            {
                new StatusEffect { Relation = Occupant.Self, Status = "Power", Amount = 2 },
                new DamageEffect { Relation = Occupant.Enemy, Amount = 1 }
            }
        });
        RegisterUnit(new AbilityConfig
        {
            Id = Shove, ManaCost = 1, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new PushEffect { Distance = 5 } }
        });
        RegisterUnit(AbilityConfigs.SwordSlash, health: 10);
        RegisterUnit(AbilityConfigs.ShieldBash, health: 10);
        RegisterUnit(new AbilityConfig { Id = Blazing, ManaCost = 1 }, health: 10, passives: new List<string> { "Blaze" });
        RegisterUnit(new AbilityConfig { Id = Armored, ManaCost = 1 }, health: 10, passives: new List<string> { "Armored" });
        RegisterUnit(new AbilityConfig { Id = Retaliator, ManaCost = 1 }, health: 10, passives: new List<string> { "Retaliate" });
        RegisterUnit(new AbilityConfig
        {
            Id = Avenger, ManaCost = 1, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new DamageEffect { Amount = 3 } }
        }, passives: new List<string> { Vengeance });
        RegisterUnit(new AbilityConfig { Id = Volatile, ManaCost = 1 }, passives: new List<string> { Explode });
        RegisterUnit(new AbilityConfig { Id = Scavenger, ManaCost = 1 }, health: 10, passives: new List<string> { Feast });
        RegisterCostlyUnit();
        RegisterKits();
    }

    private static void RegisterKits()
    {
        Register(AbilityConfigs.MoveNimble);
        Register(AbilityConfigs.MoveStraight3Step);
        Register(new AbilityConfig
        {
            Id = Lead, ManaCost = 1, Keywords = Keyword.Opener, Targeting = AdjacentTile(),
            Effects = new List<AbilityEffect> { new MoveEffect() }
        });
        Register(new AbilityConfig
        {
            Id = Dash, ManaCost = 1, Keywords = Keyword.Finisher, Targeting = AdjacentTile(),
            Effects = new List<AbilityEffect> { new MoveEffect() }
        });
        Register(new AbilityConfig
        {
            Id = Double, ManaCost = 1, Keywords = Keyword.Double, Targeting = AdjacentTile(Occupant.Any),
            Effects = new List<AbilityEffect> { new DamageEffect { Amount = 1 } }
        });
        RegisterKit(DashKit, Hit3, Dash);
        RegisterKit(LeadKit, Lead, Hit3);
        Register(AbilityConfigs.FlameStep);
        RegisterKit(FlameStepKit, AbilityConfigs.FlameStep.Id);
        Register(AbilityConfigs.AxeSwing);
        RegisterKit(AxeSwingKit, AbilityConfigs.AxeSwing.Id);
        RegisterKit(DoubleKit, Hit3, Double);
        RegisterKit(NimbleKit, AbilityConfigs.MoveNimble.Id, Hit3);
        RegisterKit(GroundedKit, AbilityConfigs.MoveStraight3Step.Id, Hit3);
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
            new GridTraversal { Direction = GridDirection.Straight, MaxDistance = steps, Linear = false, Passthrough = Occupant.Any }
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
                    Direction = GridDirection.Straight, MaxDistance = 3, Linear = true, Passthrough = Occupant.Any
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
                    Direction = GridDirection.Straight, MaxDistance = 1, Linear = false, Passthrough = Occupant.Any
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
                    Direction = GridDirection.Straight, MaxDistance = 4, Linear = true, Passthrough = Occupant.Any
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
                    Direction = GridDirection.Line, MaxDistance = 4, Linear = false, Passthrough = Occupant.Any
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
                    Direction = GridDirection.Straight, MaxDistance = 1, Linear = false, Passthrough = Occupant.Any
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
                    Direction = GridDirection.Straight, MaxDistance = 1, Linear = false, Passthrough = passthrough
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
