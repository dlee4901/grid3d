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
        if (!holder.TryGetComponent<TraitComponent>(out var traits)) return;
        var self = holder == e.Subject;
        foreach (var trait in new List<Trait>(traits.All))
        {
            var trigger = trait.Trigger;
            if (trigger == null || trigger.On != e.On) continue;
            if (!(self && e.On == GameEvent.Died) && !IsOnBoard(holder)) continue;
            var origin = new GridSource(_state, holder.Position, holder);
            if (!Hears(trigger, origin, e.Subject)) continue;
            var ctx = new EffectContext(origin, null, trait, e);
            if (!trigger.When.Eval(ctx)) continue;

            GridSteps steps;
            if (trait.Area != null)
                steps = trait.Area.GetGridSteps(origin);
            else if (self)
                steps = Participants(holder, e.Other, e.Distance);
            else
                steps = Participants(holder, e.Subject, holder.Position.StepsTo(e.Subject.Position));

            EffectResolver.Run(ctx, steps, trait.Effects);
        }
    }

    private bool IsOnBoard(Entity entity) => _state.GetEntity(entity.Position) == entity;

    private static bool Hears(Trigger trigger, GridSource origin, Entity subject)
    {
        if ((trigger.Subject & Occupants.Between(origin.Entity, subject)) == 0) return false;
        if (trigger.Range == null || origin.Entity == subject) return true;
        var range = trigger.Range.GetGridSteps(origin);
        for (var group = 0; group < range.GroupCount; group++)
            if (range.Contains(subject.Position, group)) return true;
        return false;
    }

    private GridSteps Participants(Entity holder, Entity other, int distance)
    {
        var steps = new GridSteps();
        if (IsOnBoard(holder)) steps.Add(new GridStep(holder.Position, GridDirection.Line, 0));
        if (other != null && other != holder && IsOnBoard(other))
            steps.Add(new GridStep(other.Position, GridDirection.Line, distance));
        return steps;
    }
}
