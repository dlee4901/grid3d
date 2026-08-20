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

    static Ability()
    {
        MemberRegistry<Ability>.Register<string>("Id", e => e.Id);
        MemberRegistry<Ability>.Register<int>("Cost", e => e.ManaCost);
    }

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

    public virtual bool Execute(GridState state, GridPosition sourcePosition, List<GridPosition> targetPositions)
    {
        var source = state.GetEntity(sourcePosition);
        if (source == null) return false;
        
        var gridSource = new GridSource(state, sourcePosition, source);
        if (!Targeting.GetEffectSteps(gridSource, targetPositions, out var steps)) return false;
        
        var ctx = new EffectContext(state, source, sourcePosition);
        for (var group = 0; group < steps.GroupCount; group++)
        {
            var affected = new HashSet<Entity>();
            foreach (var position in steps.GetPositions(group))
                if (state.TryGetEntity(position, out var entity))
                    affected.Add(entity);
  
            var ordered = state.OrderByPriority(affected);
            foreach (var effect in Effects) foreach (var target in ordered) effect.Apply(ctx, target);
        }

        return true;
    }


    public void Trigger()
    {
        if (Cooldown != 0) return;
        Cooldown = Config.Cooldown;
    }

    public void TurnUpdate()
    {
        if (Cooldown > 0) Cooldown--;
    }
}
