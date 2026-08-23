using System.Collections.Generic;

public sealed class AbilityCommand : ICommand
{
    public int IssuingPlayer { get; }
    public bool RequiresActiveTurn => true;
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

    public bool ApplyTo(GridState state)
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
        if (ability.Cooldown != 0)
        {
            GridLog.Warning($"[AbilityCommand] {AbilityId} on cooldown ({ability.Cooldown})");
            return false;
        }
        if (!state.HasMana(IssuingPlayer, ability.ManaCost))
        {
            GridLog.Warning($"[AbilityCommand] player {IssuingPlayer} lacks mana for {AbilityId} " +
                            $"(have {state.GetMana(IssuingPlayer)}, need {ability.ManaCost})");
            return false;
        }
        if (!ability.Execute(state, sourcePosition, targetPositions)) return false;
        state.SpendMana(IssuingPlayer, ability.ManaCost);
        ability.Trigger();
        state.ResolutionOrder.MoveToFront(entity);
        return true;
    }
}
