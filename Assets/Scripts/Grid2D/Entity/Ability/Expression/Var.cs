public static class Var
{
    public static readonly Value Distance = new VarValue(ctx => ctx.Distance);
    public static readonly Value Count = new VarValue(ctx => ctx.Triggered?.Count ?? 0);

    public static Value TargetStatus(string id) => new VarValue(ctx => StacksOf(ctx.Target, id));

    public static Value SourceCounter(string ability) => new VarValue(ctx => CounterOf(ctx.Source, ability));

    public static Value TargetCounter(string ability) => new VarValue(ctx => CounterOf(ctx.Target, ability));

    private static int CounterOf(Entity entity, string ability)
    {
        if (entity == null || !entity.TryGetComponent<AbilityComponent>(out var abilities)) return 0;
        if (!abilities.Dictionary.TryGetValue(ability, out var found)) return 0;
        return found.CounterFor(entity);
    }

    private static int StacksOf(Entity entity, string id)
    {
        if (entity == null || !entity.TryGetComponent<StatusComponent>(out var statuses)) return 0;
        foreach (var status in statuses.Statuses)
            if (status.Id == id) return status.Count;
        return 0;
    }
}
