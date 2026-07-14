using System.Collections.Generic;

// The narrow rendering surface the PlayerInput layer draws onto (previews + target highlights).
// Implemented by GridRenderer; held by PlayerInputContext so the state machine never depends on GridManager.
public interface IGridRenderer
{
    void HighlightAvailableEntities();
    void HighlightAbilityRange(Ability ability, QueryContext ctx);
    void HighlightSelectableTargets(Ability ability, QueryContext ctx);
    void ClearHighlights();
}
