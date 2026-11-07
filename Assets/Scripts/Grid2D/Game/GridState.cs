using System;
using System.Collections.Generic;

public enum GameOutcome { InProgress, Draw, Victory }

public sealed class GridState
{
    public GridDefinition Definition { get; }
    public int Turn { get; private set; } = 1;
    public int ActivePlayer { get; private set; } = 1;
    public GameOutcome Outcome { get; private set; } = GameOutcome.InProgress;
    public int Winner { get; private set; }
    public bool IsGameOver => Outcome != GameOutcome.InProgress;
    public bool CanPlayerAct(int player) => !IsGameOver && ActivePlayer == player;

    public int X => Definition.X;
    public int Y => Definition.Y;
    public int Size => Definition.Size;
    public ResolutionOrder ResolutionOrder { get; } = new();
    
    private readonly Entity[] _entities;
    private readonly int[] _mana;
    private readonly int[] _timeMs;
    private int _consecutivePasses;
    private int _nextUid;
    
    private readonly TeamData[] _loadedTeams;
    
    private int PlayerSlots => Definition.PlayerCount + 1;
    
    public GridState(GridDefinition definition)
    {
        Definition = definition;
        _entities = new Entity[definition.Size];
        _mana = new int[PlayerSlots];
        _loadedTeams = new TeamData[PlayerSlots];
        
        RefillMana(ActivePlayer);
        _timeMs = new int[PlayerSlots];
        for (var p = 1; p <= Definition.PlayerCount; p++) _timeMs[p] = definition.PlayerStartingTimeSeconds * 1000;
        SeedStartingEntities();
    }
    
    internal void AdvanceTurn()
    {
        _consecutivePasses = 0;
        StartNextTurn();
        ResolveForcedPasses();
    }

    internal void ResolveForcedPasses()
    {
        while (!IsGameOver && GetAvailableEntityPositions().Count == 0)
        {
            _consecutivePasses++;
            if (_consecutivePasses >= Definition.PlayerCount)
            {
                Outcome = GameOutcome.Draw;
                return;
            }
            StartNextTurn();
        }
    }

    internal bool Forfeit(int player)
    {
        if (IsGameOver || player < 1 || player > Definition.PlayerCount || Definition.PlayerCount != 2) return false;
        Outcome = GameOutcome.Victory;
        Winner = player % Definition.PlayerCount + 1;
        return true;
    }

    private void StartNextTurn()
    {
        ActivePlayer = ActivePlayer % Definition.PlayerCount + 1;
        Turn++;
        RefillMana(ActivePlayer);
        TickAbilityCooldowns(ActivePlayer);
    }

    public int GetMana(int player) => player < 1 || player > Definition.PlayerCount ? 0 : _mana[player];

    public bool HasMana(int player, int cost) => GetMana(player) >= cost;

    internal bool SpendMana(int player, int cost)
    {
        if (!HasMana(player, cost)) return false;
        _mana[player] -= cost;
        return true;
    }

    private void RefillMana(int player) => _mana[player] = Definition.ManaPerTurn;

    public int GetTimeMs(int player) => player < 1 || player > Definition.PlayerCount ? 0 : _timeMs[player];

    internal void SpendTime(int player, int elapsedMs)
    {
        if (player < 1 || player > Definition.PlayerCount) return;
        _timeMs[player] = System.Math.Max(0, _timeMs[player] - System.Math.Max(0, elapsedMs));
    }

    public int GetSpawnPlayer(GridPosition position) => Definition.GetSpawnPlayer(position);

    private void TickAbilityCooldowns(int player)
    {
        for (var i = 0; i < _entities.Length; i++)
        {
            var entity = _entities[i];
            if (entity == null) continue;
            if (!entity.TryGetComponent<ControlComponent>(out var control) 
                || control.PlayerController != player) continue;
            if (!entity.TryGetComponent<AbilityComponent>(out var abilities)) continue;
            abilities.ResetTurn();
            foreach (var ability in abilities.List) ability.TurnUpdate();
        }
    }
    
    private void SeedStartingEntities()
    {
        if (Definition.EntityStartPositions == null) return;
        foreach (var spec in Definition.EntityStartPositions)
        {
            if (!IdRegistry<EntityConfig>.TryGet(spec.EntityId, out var entityConfig)) continue;
            foreach (var position in Definition.ResolvePositions(spec))
            {
                if (!IsTraversable(position)) continue;
                SetEntityPosition(position, Entity.Create(entityConfig, Definition.GetSpawnPlayer(position)));
            }
        }
    }
    
    internal void SpawnTeams()
    {
        var maxUnits = 0;
        for (var player = 1; player <= Definition.PlayerCount; player++)
        {
            var team = _loadedTeams[player];
            if (team != null && team.UnitStartPositions.Count > maxUnits) maxUnits =
                team.UnitStartPositions.Count;
        }

        for (var unit = 0; unit < maxUnits; unit++)
        for (var player = Definition.PlayerCount; player >= 1; player--)
        {
            var team = _loadedTeams[player];
            if (team == null || unit >= team.UnitStartPositions.Count) continue;

            var placement = team.UnitStartPositions[unit];
            if (!IdRegistry<EntityConfig>.TryGet(placement.UnitId, out var config)) continue;
            SetEntityPosition(new GridPosition(this, placement.Position), Entity.Create(config, player));
        }
    }
    
    internal void LoadPlayerTeam(int player, TeamData teamData)
    {
        if (player < 1 || player > Definition.PlayerCount) return;
        if (!ValidatePlayerTeam(player, teamData)) return;
        _loadedTeams[player] = teamData;
    }


    private bool ValidatePlayerTeam(int player, TeamData teamData)
    {
        if (teamData == null || teamData.MapId != Definition.Id) return false;
        foreach (var unit in teamData.UnitStartPositions)
        {
            var gridPosition = new GridPosition(this, unit.Position);
            if (!gridPosition.IsValid()
                || Definition.GetSpawnPlayer(gridPosition) != player
                || _entities[gridPosition.Dim1] != null
                || !IdRegistry<EntityConfig>.TryGet(unit.UnitId, out _)) return false;
        }
        return true;
    }
    
    public Entity GetEntity(int position)
    {
        return position >= 0 && position < _entities.Length ? _entities[position] : null;
    }
    
    public Entity GetEntity(GridPosition position)
    {
        return position.IsValid() ? _entities[position.Dim1] : null;
    }

    public Entity GetEntityByUid(int uid)
    {
        foreach (var entity in _entities)
            if (entity != null && entity.Uid == uid) return entity;
        return null;
    }

    public bool TryGetEntity(GridPosition position, out Entity entity)
    {
        if (!position.IsValid() || _entities[position.Dim1] == null)
        {
            entity = null;
            return false;
        }
        entity = _entities[position.Dim1];
        return true;
    }

    public bool TryGetTerrain(GridPosition position, out TerrainType terrainType)
    {
        if (!position.IsValid())
        {
            terrainType = TerrainType.Default;
            return false;
        }
        terrainType = Definition.TerrainMap[position.Dim1];
        return true;
    }
    
    public HashSet<GridPosition> GetControllableEntityPositions() => GetControllableEntityPositions(ActivePlayer);

    public HashSet<GridPosition> GetControllableEntityPositions(int player) => CollectPositions(p => IsControllable(p, player));

    public HashSet<GridPosition> GetAvailableEntityPositions() => GetAvailableEntityPositions(ActivePlayer);

    public HashSet<GridPosition> GetAvailableEntityPositions(int player) => CollectPositions(p => IsAvailable(p, player));

    private HashSet<GridPosition> CollectPositions(Func<GridPosition, bool> predicate)
    {
        var positions = new HashSet<GridPosition>();
        for (var i = 0; i < _entities.Length; i++)
        {
            var gridPosition = new GridPosition(this, i);
            if (predicate(gridPosition)) positions.Add(gridPosition);
        }
        return positions;
    }
    
    public HashSet<GridPosition> GetOccupiedEntityPositions()
    {
        var positions = new HashSet<GridPosition>();
        for (var i = 0; i < _entities.Length; i++) if (_entities[i] != null) positions.Add(new GridPosition(this, i));
        return positions;
    }

    public bool IsControllable(GridPosition position, int player)
    {
        if (!position.IsValid()) return false;
        var entity = _entities[position.Dim1];
        if (entity == null) return false;
        if (!entity.TryGetComponent<ControlComponent>(out var control)) return false;
        return control.PlayerController == player;
    }

    public bool IsAvailable(GridPosition position, int player)
        => CanPlayerAct(player) && IsControllable(position, player) && HasCastableAbility(_entities[position.Dim1], player);

    public bool CanCast(Entity entity, Ability ability, int player)
        => !ability.Locked && ability.Cooldown == 0 && HasMana(player, ability.ManaCost)
           && !(entity.TryGetComponent<AbilityComponent>(out var abilities) && abilities.HasActed);

    public bool HasCastableAbility(Entity entity, int player)
    {
        if (!entity.TryGetComponent<AbilityComponent>(out var abilities)) return false;
        foreach (var ability in abilities.List)
            if (CanCast(entity, ability, player)) return true;
        return false;
    }
    
    public HashSet<int> GetOccupiedTilesPositionSet()
    {
        HashSet<int> indices = new();
        for (var i = 0; i < Definition.Size; i++)
        {
            if (_entities[i] != null) indices.Add(i);
        }
        return indices;
    }
    
    internal bool SetEntityPosition(GridPosition position, Entity entity)
    {
        if (entity == null || !IsTraversable(position)) return false;
        entity.Uid = ++_nextUid;
        _entities[position.Dim1] = entity;
        entity.SetPosition(position);
        ResolutionOrder.Add(entity);
        return true;
    }
    
    internal bool ChangeEntityPosition(GridPosition startPosition, GridPosition targetPosition)
    {
        if (!TryGetEntity(startPosition, out var entity) || !IsTraversable(targetPosition)) return false;
        _entities[targetPosition.Dim1] = entity;
        _entities[startPosition.Dim1] = null;
        entity.SetPosition(targetPosition);
        return true;
    }
    
    internal bool RemoveEntity(GridPosition position)
    {
        if (!position.IsValid()) return false;
        var entity = _entities[position.Dim1];
        if (entity == null) return false;
        _entities[position.Dim1] = null;
        ResolutionOrder.Remove(entity);
        return true;
    }

    // TODO: Terrain check
    public bool IsTraversable(GridPosition position)
    {
        return position.IsValid() && _entities[position.Dim1] == null && Definition.TerrainMap[position.Dim1] != TerrainType.Void; 
    }

    public ulong ComputeHash()
    {
        var hash = StateHasher.Create();
        hash.Add(Turn);
        hash.Add(ActivePlayer);
        hash.Add((int)Outcome);
        hash.Add(Winner);
        hash.Add(_consecutivePasses);
        hash.Add(_nextUid);
        for (var player = 1; player <= Definition.PlayerCount; player++)
        {
            hash.Add(_mana[player]);
            hash.Add(_timeMs[player]);
        }
        foreach (var entity in _entities) AddEntity(ref hash, entity);
        hash.Add(ResolutionOrder.Entities.Count);
        foreach (var entity in ResolutionOrder.Entities) hash.Add(entity.Position.Dim1);
        return hash.Value;
    }

    private static void AddEntity(ref StateHasher hash, Entity entity)
    {
        hash.Add(entity != null);
        if (entity == null) return;
        hash.Add(entity.Id);
        hash.Add(entity.Uid);
        hash.Add(entity.Position.Dim1);
        hash.Add((int)entity.Facing);

        var hasHealth = entity.TryGetComponent<HealthComponent>(out var health);
        hash.Add(hasHealth);
        if (hasHealth)
        {
            hash.Add(health.Starting);
            hash.Add(health.Current);
        }

        var hasControl = entity.TryGetComponent<ControlComponent>(out var control);
        hash.Add(hasControl);
        if (hasControl) hash.Add(control.PlayerController);

        var hasAbilities = entity.TryGetComponent<AbilityComponent>(out var abilities);
        hash.Add(hasAbilities);
        if (!hasAbilities) return;
        hash.Add(abilities.List.Count);
        hash.Add(abilities.HasActed);
        foreach (var ability in abilities.List)
        {
            hash.Add(ability.Id);
            hash.Add(ability.Cooldown);
            hash.Add(ability.Delay);
            hash.Add(ability.Locked);
        }
    }

    public string PrintGrid()
    {
        var grid = "";

        grid += "START POSITIONS\n";
        foreach (var kvp in Definition.SpawnPlayers)
            grid += "(" + kvp.Key + "-" + kvp.Value + ") ";
        grid += "\n";

        grid += "ENTITIES\n";
        for (var i = 0; i < Definition.Size; i++)
            if (_entities[i] != null)
                grid += "(" + i + " " + _entities[i].Id + ") ";
        grid += "\n";

        return grid;
    }
}
