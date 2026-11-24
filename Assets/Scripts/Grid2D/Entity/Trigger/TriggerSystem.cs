using System.Collections.Generic;

internal sealed class TriggerSystem
{
    private const int MaxEventsPerResolve = 256;
    private readonly GridState _state;
    private readonly Queue<TriggerEvent> _queue = new();

    internal TriggerSystem(GridState state) => _state = state;

    internal void Emit(TriggerEvent e) => _queue.Enqueue(e);

    internal void Fire(TriggerEvent e)
    {
        Run(e.Subject, e);
        foreach (var holder in new List<Entity>(_state.ResolutionOrder.Entities))
            if (holder != e.Subject) Run(holder, e);
    }

    internal void Resolve()
    {
        for (var i = 0; i < MaxEventsPerResolve; i++)
        {
            if (_queue.Count == 0) return;
            Fire(_queue.Dequeue());
        }
        _queue.Clear();
        GridLog.Warning("[TriggerSystem] event cap reached");
    }

    private void Run(Entity holder, TriggerEvent e)
    {
        var self = holder == e.Subject;
        foreach (var triggered in new List<ITriggered>(Of(holder)))
        {
            var trigger = triggered.Trigger;
            if (trigger == null || trigger.On != e.On) continue;
            var ability = ReactionInstance.OwnerOf(triggered) as Ability;
            if (ability != null && (ability.Cooldown > 0 || ability.Locked)) continue;
            if (!(self && e.On == GameEvent.Died) && !IsOnBoard(holder)) continue;
            var origin = new GridSource(_state, holder.Position, holder);
            if (!Hears(trigger, origin, e.Subject, self ? e.Other : e.Subject)) continue;
            var ctx = new EffectContext(origin, null, triggered, e);
            if (!trigger.When.Eval(ctx)) continue;

            GridSteps steps;
            if (triggered.Area != null)
                steps = triggered.Area.GetGridSteps(origin);
            else if (self)
                steps = Participants(holder, e.Other, e.Distance);
            else
                steps = Participants(holder, e.Subject, holder.Position.StepsTo(e.Subject.Position));

            if (ability != null && ability.Config.Cooldown > 0)
            {
                if (!EffectResolver.AffectsAny(ctx, steps, triggered.Effects)) continue;
                ability.RecordCast();
            }
            EffectResolver.Run(ctx, steps, triggered.Effects);
        }
    }

    internal static IEnumerable<ITriggered> Of(Entity holder)
    {
        if (holder.TryGetComponent<AbilityComponent>(out var abilities))
            foreach (var ability in abilities.List)
            {
                if (ability.Trigger != null) yield return ability;
                foreach (var reaction in ability.Config.Reactions) yield return new ReactionInstance(ability, reaction);
            }
        if (holder.TryGetComponent<StatusComponent>(out var statuses))
            foreach (var status in statuses.Statuses)
            {
                yield return status;
                foreach (var reaction in status.Config.Reactions) yield return new ReactionInstance(status, reaction);
            }
    }

    private bool IsOnBoard(Entity entity) => _state.GetEntity(entity.Position) == entity;

    private static bool Hears(Trigger trigger, GridSource origin, Entity subject, Entity counterpart)
    {
        if ((trigger.Subject & Occupants.Between(origin.Entity, subject)) == 0) return false;
        if (trigger.Range == null || counterpart == null || counterpart == origin.Entity) return true;
        var range = trigger.Range.GetGridSteps(origin);
        for (var group = 0; group < range.GroupCount; group++)
            if (range.Contains(counterpart.Position, group)) return true;
        return false;
    }

    private GridSteps Participants(Entity holder, Entity other, int distance)
    {
        var steps = new GridSteps();
        if (IsOnBoard(holder)) steps.Add(new GridStep(holder.Position, GridDirection.All, 0));
        if (other != null && other != holder && IsOnBoard(other))
            steps.Add(new GridStep(other.Position, GridDirection.All, distance));
        return steps;
    }
}
