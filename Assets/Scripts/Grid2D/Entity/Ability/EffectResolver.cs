using System.Collections.Generic;

internal static class EffectResolver
{
    internal static void Run(in EffectContext cast, GridSteps steps, List<AbilityEffect> effects)
    {
        if (effects == null || effects.Count == 0) return;
        var state = cast.State;
        for (var group = 0; group < steps.GroupCount; group++)
        {
            var slots = Slots(state, steps, group);
            foreach (var effect in effects)
            foreach (var (position, slotTarget, distance) in slots)
            {
                var target = slotTarget;
                if (target != null && state.GetEntity(target.Position) != target) target = null;
                var ctx = cast.At(target?.Position ?? position, target, distance);
                if (effect.Affects(ctx)) effect.Apply(ctx);
            }
        }
    }

    internal static bool AffectsAny(in EffectContext cast, GridSteps steps, List<AbilityEffect> effects)
    {
        if (effects == null) return false;
        for (var group = 0; group < steps.GroupCount; group++)
        foreach (var (position, target, distance) in Slots(cast.State, steps, group))
        foreach (var effect in effects)
            if (effect.Affects(cast.At(position, target, distance))) return true;
        return false;
    }

    private static List<(GridPosition, Entity, int)> Slots(GridState state, GridSteps steps, int group)
    {
        var occupied = new HashSet<Entity>();
        var empty = new List<GridPosition>();
        foreach (var position in steps.GetPositions(group))
            if (state.TryGetEntity(position, out var entity)) occupied.Add(entity);
            else empty.Add(position);
        empty.Sort((a, b) => a.Dim1.CompareTo(b.Dim1));

        var slots = new List<(GridPosition, Entity, int)>();
        foreach (var entity in state.ResolutionOrder.GetOrderByPriority(occupied))
            slots.Add((entity.Position, entity, steps.GetShortestDistance(entity.Position, group)));
        foreach (var position in empty)
            slots.Add((position, null, steps.GetShortestDistance(position, group)));
        return slots;
    }
}
