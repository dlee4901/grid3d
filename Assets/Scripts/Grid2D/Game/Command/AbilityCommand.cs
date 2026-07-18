public sealed class AbilityCommand : ICommand
{
    public int IssuingPlayer { get; }
    public bool RequiresActiveTurn => true;
    public string AbilityId { get; }
    public int SourcePosition { get; }
    public int[] Targets { get; }

    public AbilityCommand(int issuingPlayer, string abilityId, int sourcePosition, int[] targets)
    {
        IssuingPlayer = issuingPlayer;
        AbilityId = abilityId;
        SourcePosition = sourcePosition;
        Targets = targets;
    }

    public bool ApplyTo(GridState state)
    {
        var entity = state.GetEntity(SourcePosition);
        if (entity == null)
        {
            GridLog.Warning($"[AbilityCommand] no entity at {SourcePosition}");
            return false;
        }
        if (!entity.TryGetComponent<ControlComponent>(out var control)
            || control.PlayerController != IssuingPlayer)
        {
            GridLog.Warning($"[AbilityCommand] player {IssuingPlayer} can't command entity at {SourcePosition}");
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
        if (!ability.Execute(state, SourcePosition, Targets)) return false;
        state.SpendMana(IssuingPlayer, ability.ManaCost);
        ability.Trigger();
        return true;
    }
}
