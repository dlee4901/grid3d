using System.Collections.Generic;

public sealed class AbilityCommand : Command
{
    public override int IssuingPlayer { get; }
    public override bool RequiresActiveTurn => true;
    public string AbilityId { get; }
    public int SourcePosition { get; }
    public int[] TargetPositions { get; }

    public AbilityCommand(int issuingPlayer, string abilityId, int sourcePosition, int[] targetPositions)
    {
        IssuingPlayer = issuingPlayer;
        AbilityId = abilityId;
        SourcePosition = sourcePosition;
        TargetPositions = targetPositions;
    }

    internal override bool ApplyTo(GridState state)
    {
        var sourcePosition = new GridPosition(state, SourcePosition);
        var targetPositions = new List<GridPosition>();
        foreach (var position in TargetPositions) targetPositions.Add(new GridPosition(state, position));
        
        var entity = state.GetEntity(sourcePosition);
        if (entity == null)
        {
            GridLog.Warning($"[AbilityCommand] no entity at {sourcePosition}");
            return false;
        }
        if (!entity.TryGetComponent<ControlComponent>(out var control)
            || control.PlayerController != IssuingPlayer)
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
        if (!state.CanCast(ability, IssuingPlayer))
        {
            GridLog.Warning($"[AbilityCommand] {AbilityId} not castable by player {IssuingPlayer} " +
                            $"(locked {ability.Locked}, cooldown {ability.Cooldown}, " +
                            $"mana {state.GetMana(IssuingPlayer)}/{ability.ManaCost})");
            return false;
        }
        if (!ability.Execute(state, sourcePosition, targetPositions)) return false;
        state.SpendMana(IssuingPlayer, ability.ManaCost);
        ability.Trigger();
        state.ResolutionOrder.MoveToFront(entity);
        return true;
    }
}
