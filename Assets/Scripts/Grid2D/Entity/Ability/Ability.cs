using System.Collections.Generic;

public class Ability : INameId
{
    public AbilityConfig Config { get; }

    public string Id => Config.Id;
    public int ManaCost => Config.ManaCost;
    public AbilityTargeting Targeting => Config.Targeting;
    public List<AbilityEffect> Effects => Config.Effects;
    //public List<AbilityBindings> Bindings => Config.Bindings;

    public int Cooldown { get; private set; }
    public int Delay { get; private set; }
    public bool Locked { get; private set; }

    public static Ability Create(AbilityConfig config) => config.Type switch
    {
        AbilityType.Move => new MoveAbility(config),
        _                => new Ability(config),
    };

    protected Ability(AbilityConfig config)
    {
        Config = config;
        Cooldown = config.Warmup;
        Delay = config.Delay;
        Locked = config.Locked;
    }

    internal virtual bool Execute(GridState state, GridPosition sourcePosition, List<GridPosition> targetPositions)
    {
        var source = state.GetEntity(sourcePosition);
        if (source == null) return false;
        
        var gridSource = new GridSource(state, sourcePosition, source);
        if (!Targeting.GetEffectSteps(gridSource, targetPositions, out var steps)) return false;
        
        var cast = new EffectContext(state, source, sourcePosition, this);
        for (var group = 0; group < steps.GroupCount; group++)
        {
            var affected = new HashSet<Entity>();
            var distances = new Dictionary<Entity, int>();
            foreach (var position in steps.GetPositions(group))
                if (state.TryGetEntity(position, out var entity))
                {
                    affected.Add(entity);
                    distances[entity] = steps.GetShortestDistance(position, group);
                }
  
            var ordered = state.ResolutionOrder.GetOrderByPriority(affected);
            foreach (var effect in Effects)
            foreach (var target in ordered)
            {
                if (state.GetEntity(target.Position) != target) continue;
                var ctx = cast.For(target, distances[target]);
                if (effect.Affects(ctx)) effect.Apply(ctx);
            }
        }

        return true;
    }


    internal void Trigger()
    {
        if (Cooldown != 0) return;
        Cooldown = Config.Cooldown;
    }

    internal void ApplyCooldown(int turns)
    {
        if (turns > Cooldown) Cooldown = turns;
    }

    internal void TurnUpdate()
    {
        if (Cooldown > 0) Cooldown--;
    }
}
