using System.Collections.Generic;
using System.Linq;

public class TargetingDisplay
{
    private readonly GridManager _manager;
    private readonly HighlightRenderer _highlights;

    public TargetingDisplay(GridManager manager, HighlightRenderer highlights)
    {
        _manager = manager;
        _highlights = highlights;
    }

    public void ShowSources(GridPosition? selected, HashSet<GridPosition> available)
    {
        _manager.ClearPreview();
        _highlights.Clear();
        _highlights.SetSelected(selected);
        if (available != null) _highlights.Highlight(available, HighlightType.AvailableSources);
    }

    public void Show(TargetingSnapshot snapshot)
    {
        var targeting = snapshot.Ability.Targeting;
        _highlights.SetSelected(snapshot.Source.Position);
        _highlights.Clear();
        if (targeting == null)
        {
            _manager.ClearPreview();
            _highlights.Highlight(snapshot.Selectable, HighlightType.EffectArea);
            return;
        }
        _highlights.Highlight(snapshot.Selectable, HighlightType.PossibleTargets);
        _highlights.Highlight(snapshot.Valid, HighlightType.AvailableTargets);
        if (targeting is DirectionTargeting) _highlights.DirectionChevrons(snapshot.Valid);
        if (snapshot.NextSteps != null) _highlights.DirectionChevrons(snapshot.NextSteps);
        if (snapshot.TargetCount == 0)
        {
            _manager.ClearPreview();
            return;
        }
        if (targeting.GetEffectSteps(snapshot.Source, snapshot.LastTarget, out var area))
        {
            if (targeting is DirectionTargeting { MaxHits: > 0 }) ShowProjectiles(snapshot.Source.Position, area);
            else _highlights.Highlight(area, HighlightType.EffectArea);
        }
        if (snapshot.TargetCount != targeting.Targets
            || !_manager.Preview(seat => AbilityCommand.From(seat, snapshot.Ability.Id, snapshot.Source.Position, snapshot.Targets, snapshot.Path)))
        {
            _manager.ClearPreview();
            return;
        }
        _highlights.Highlight(new HashSet<GridPosition> { snapshot.LastTarget }, HighlightType.ConfirmTarget);
    }

    private void ShowProjectiles(GridPosition source, GridSteps steps)
    {
        foreach (var ray in steps.GetSteps().GroupBy(step => step.Direction))
        {
            var impact = ray.OrderBy(step => step.Distance).Last().Position;
            _highlights.Highlight(new HashSet<GridPosition> { impact }, HighlightType.EffectArea);
            _highlights.Projectile(source, impact);
        }
    }
}
