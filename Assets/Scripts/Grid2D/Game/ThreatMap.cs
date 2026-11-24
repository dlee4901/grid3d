using System.Collections.Generic;

public static class ThreatMap
{
    private const int MaxCasts = 4;

    public static HashSet<GridPosition> For(GridState state, int threatenedPlayer)
    {
        var threat = new HashSet<GridPosition>();
        foreach (var unit in state.ResolutionOrder.Entities)
        {
            if (!unit.TryGetComponent<ControlComponent>(out var control)) continue;
            if (control.PlayerController == 0 || control.PlayerController == threatenedPlayer) continue;
            if (!unit.TryGetComponent<AbilityComponent>(out var abilities)) continue;
            new Search(state, unit, abilities.List, threat).Run();
        }
        return threat;
    }

    private sealed class Search
    {
        private readonly GridState _state;
        private readonly Entity _unit;
        private readonly IReadOnlyList<Ability> _abilities;
        private readonly HashSet<GridPosition> _threat;
        private readonly HashSet<(int, int)> _visited = new();

        public Search(GridState state, Entity unit, IReadOnlyList<Ability> abilities, HashSet<GridPosition> threat)
        {
            _state = state;
            _unit = unit;
            _abilities = abilities;
            _threat = threat;
        }

        public void Run()
        {
            var ready = 0;
            for (var i = 0; i < _abilities.Count; i++)
                if (_abilities[i].Cooldown == 0 && !_abilities[i].Locked) ready |= 1 << i;
            Explore(_unit.Position, ready, 0);
        }

        private void Explore(GridPosition at, int ready, int casts)
        {
            if (casts >= MaxCasts || !_visited.Add((at.Dim1, ready))) return;
            var source = new GridSource(_state, at, _unit);
            for (var i = 0; i < _abilities.Count; i++)
            {
                var ability = _abilities[i];
                if ((ready & (1 << i)) == 0 || ability.Targeting == null) continue;
                var continues = casts == 0 && (ability.Keywords & Keyword.Opener) != 0;
                var nextReady = ability.Config.Cooldown == 0 ? ready : ready & ~(1 << i);
                var selectable = ability.Targeting.GetSelectableSteps(source);
                foreach (var target in AllPositions(selectable))
                {
                    if (IsMove(ability))
                    {
                        if (continues) Explore(target, nextReady, casts + 1);
                        continue;
                    }
                    if (Threatens(ability) && ability.Targeting.GetEffectSteps(source, target, out var steps))
                        _threat.UnionWith(AllPositions(steps));
                }
                if (continues && !IsMove(ability)) Explore(at, nextReady, casts + 1);
            }
        }

        private static bool IsMove(Ability ability)
        {
            foreach (var effect in ability.Effects) if (effect is MoveEffect) return true;
            return false;
        }

        private static bool Threatens(Ability ability)
        {
            foreach (var effect in ability.Effects)
                if (effect is TerrainEffect || effect is not MoveEffect && (effect.Relation & Occupant.Enemy) != 0) return true;
            return false;
        }

        private static IEnumerable<GridPosition> AllPositions(GridSteps steps)
        {
            for (var group = 0; group < steps.GroupCount; group++)
                foreach (var position in steps.GetPositions(group)) yield return position;
        }
    }
}
