using System.Collections.Generic;

public sealed class AbilityCommand : Command
{
    public override int IssuingPlayer { get; }
    public override bool RequiresActiveTurn => true;
    public string AbilityId { get; }
    public int SourcePosition { get; }
    public int[] TargetPositions { get; }
    public int[] Path { get; }
    public int ElapsedMs { get; }

    public AbilityCommand(int issuingPlayer, string abilityId, int sourcePosition, int[] targetPositions, int[] path = null,
                          int elapsedMs = 0)
    {
        IssuingPlayer = issuingPlayer;
        AbilityId = abilityId;
        SourcePosition = sourcePosition;
        TargetPositions = targetPositions;
        Path = path is { Length: > 0 } ? path : null;
        ElapsedMs = elapsedMs;
    }

    public static AbilityCommand From(int issuingPlayer, string abilityId, GridPosition source,
                                      IReadOnlyList<GridPosition> targets, IReadOnlyList<GridPosition> path = null,
                                      int elapsedMs = 0)
        => new(issuingPlayer, abilityId, source.Dim1, Indices(targets), path == null ? null : Indices(path), elapsedMs);

    private static int[] Indices(IReadOnlyList<GridPosition> positions)
    {
        var indices = new int[positions.Count];
        for (var i = 0; i < positions.Count; i++) indices[i] = positions[i].Dim1;
        return indices;
    }

    internal override bool ApplyTo(GridState state)
    {
        var sourcePosition = new GridPosition(state, SourcePosition);
        var targetPositions = new List<GridPosition>();
        foreach (var position in TargetPositions) targetPositions.Add(new GridPosition(state, position));
        List<GridPosition> path = null;
        if (Path != null)
        {
            path = new List<GridPosition>();
            foreach (var position in Path) path.Add(new GridPosition(state, position));
        }
        
        var entity = state.GetEntity(sourcePosition);
        if (entity == null)
        {
            GridLog.Warning($"[AbilityCommand] no entity at {sourcePosition}");
            return false;
        }
        if (!state.IsControllable(sourcePosition, IssuingPlayer))
        {
            GridLog.Warning($"[AbilityCommand] player {IssuingPlayer} can't command entity at {sourcePosition}");
            return false;
        }
        if (!entity.TryGetComponent<AbilityComponent>(out var abilities)) return false;
        if (!abilities.Dictionary.TryGetValue(AbilityId, out var ability))
        {
            GridLog.Warning($"[AbilityCommand] entity {entity.Id} has no ability {AbilityId}");
            return false;
        }
        if (!state.CanCast(entity, ability, IssuingPlayer))
        {
            GridLog.Warning($"[AbilityCommand] {AbilityId} not castable by player {IssuingPlayer} " +
                            $"(locked {ability.Locked}, cooldown {ability.Cooldown})");
            return false;
        }
        var first = !state.HasActedThisTurn;
        if (!ability.Execute(state, sourcePosition, targetPositions, path)) return false;
        abilities.OnCast(ability);
        state.ResolutionOrder.MoveToFront(entity);
        state.Triggers.Resolve();
        state.SpendTime(IssuingPlayer, ElapsedMs);
        if (state.IsGameOver) return true;
        if (!first || (ability.Keywords & Keyword.Opener) == 0)
        {
            state.AdvanceTurn();
            return true;
        }
        state.SetActingUnit(entity);
        if (state.GetAvailableEntityPositions().Count == 0) state.AdvanceTurn();
        return true;
    }
}
