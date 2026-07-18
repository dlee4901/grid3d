using System.Collections.Generic;

public interface IGridRenderer
{
    void HighlightAvailableEntities();
    void HighlightAbilityRange(Ability ability, QueryContext ctx);
    void HighlightSelectableTargets(Ability ability, QueryContext ctx);
    void ClearHighlights();
}
