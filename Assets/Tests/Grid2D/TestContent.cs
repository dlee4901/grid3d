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
    public const string SelfHeal = "Test.SelfHeal";

    public static readonly CountingEffect Counter = new();
    public static readonly CountingEffect AllyCounter = new() { Relation = Occupant.Ally };
    public static readonly CountingEffect NotSelfCounter = new() { Relation = Occupant.Any & ~Occupant.Self };

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
        RegisterUnit(new AbilityConfig { Id = Cheap, ManaCost = 1 });
        RegisterUnit(new AbilityConfig { Id = Expensive, ManaCost = 5 });
        RegisterUnit(new AbilityConfig { Id = Locked, ManaCost = 1, Locked = true });
        RegisterUnit(new AbilityConfig { Id = WarmingUp, ManaCost = 1, Warmup = 1 });
        RegisterUnit(new AbilityConfig { Id = Step, Type = AbilityType.Move, ManaCost = 1, Targeting = AdjacentTile() });
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
            Id = SelfHeal, ManaCost = 1, Targeting = SelfTile(),
            Effects = new List<AbilityEffect> { new HealEffect { Relation = Occupant.Self, Amount = 1 } }
        }, health: 3);
        RegisterCostlyUnit();
    }

    private static FillTargeting SelfTile() => new()
    {
        EffectArea = new GridSelection
        {
            Traversals = new List<GridTraversal>
            {
                new GridTraversal { Direction = GridDirection.Straight, MaxDistance = 0 }
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

    private static void RegisterUnit(AbilityConfig ability, int health = 1)
    {
        if (!IdRegistry<AbilityConfig>.TryGet(ability.Id, out _)) IdRegistry<AbilityConfig>.Register(ability);
        var unitId = UnitWith(ability.Id);
        if (IdRegistry<EntityConfig>.TryGet(unitId, out _)) return;
        IdRegistry<EntityConfig>.Register(new EntityConfig
        {
            Id = unitId, Health = health, Abilities = new List<string> { ability.Id }
        });
    }
}
