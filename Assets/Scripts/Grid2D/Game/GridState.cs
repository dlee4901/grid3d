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
    internal TriggerSystem Triggers { get; }
    
    private readonly Entity[] _entities;
    private readonly TerrainType[] _terrain;
    private readonly int[] _terrainTurns;
    private readonly int[] _mana;
    private readonly int[] _timeMs;
    private int _consecutivePasses;
    private int _nextUid;
    
    private readonly TeamData[] _loadedTeams;
    private readonly List<EntityMove> _moves = new();

    public IReadOnlyList<EntityMove> Moves => _moves;
    
    private int PlayerSlots => Definition.PlayerCount + 1;
    
    public GridState(GridDefinition definition)
    {
        Definition = definition;
        Triggers = new TriggerSystem(this);
        _entities = new Entity[definition.Size];
        _terrain = (TerrainType[])definition.TerrainMap.Clone();
        _terrainTurns = (int[])definition.TerrainTurns.Clone();
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
        EndAbilityTurn(ActivePlayer);
        foreach (var entity in ResolutionOrder.Entities)
            if (entity.TryGetComponent<ControlComponent>(out var control) && control.PlayerController == ActivePlayer)
                Triggers.Emit(new TriggerEvent(GameEvent.TurnEnd, entity));
        Triggers.Resolve();
        TickTerrain();
        ActivePlayer = ActivePlayer % Definition.PlayerCount + 1;
        Turn++;
        RefillMana(ActivePlayer);
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

    private void ApplyTerrainAt(GridPosition position)
    {
        var entity = GetEntity(position);
        if (entity == null || !IdRegistry<TerrainConfig>.TryGet(_terrain[position.Dim1].ToString(), out var config)) return;
        var steps = new GridSteps();
        steps.Add(new GridStep(position, GridDirection.Line, 0));
        EffectResolver.Run(new EffectContext(new GridSource(this, position, entity), null), steps, config.Effects);
    }

    private void TickTerrain()
    {
        for (var i = 0; i < _terrain.Length; i++)
        {
            if (_terrainTurns[i] <= 0) continue;
            if (--_terrainTurns[i] == 0) _terrain[i] = TerrainType.Plain;
        }
    }

    private void EndAbilityTurn(int player)
    {
        for (var i = 0; i < _entities.Length; i++)
        {
            var entity = _entities[i];
            if (entity == null) continue;
            if (!entity.TryGetComponent<ControlComponent>(out var control) 
                || control.PlayerController != player) continue;
            if (!entity.TryGetComponent<AbilityComponent>(out var abilities)) continue;
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
            terrainType = TerrainType.Plain;
            return false;
        }
        terrainType = _terrain[position.Dim1];
        return true;
    }

    internal void SetTerrain(GridPosition position, TerrainType terrain, int turns)
    {
        if (!position.IsValid()) return;
        var refresh = _terrain[position.Dim1] == terrain;
        _terrain[position.Dim1] = terrain;
        _terrainTurns[position.Dim1] = terrain == TerrainType.Plain ? 0
            : refresh ? System.Math.Max(_terrainTurns[position.Dim1], turns) : turns;
        ApplyTerrainAt(position);
    }

    public int GetTerrainTurns(GridPosition position) => position.IsValid() ? _terrainTurns[position.Dim1] : 0;
    
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
            if (gridPosition.IsValid() && predicate(gridPosition)) positions.Add(gridPosition);
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

    public bool CanCastAt(GridPosition position, Ability ability, int player)
        => CanPlayerAct(player) && IsControllable(position, player) && CanCast(_entities[position.Dim1], ability, player);

    public bool CanCast(Entity entity, Ability ability, int player)
        => !ability.Locked && ability.Cooldown == 0 && HasMana(player, ability.ManaCost);

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
        ApplyTerrainAt(position);
        return true;
    }
    
    internal void ClearMoves() => _moves.Clear();

    internal bool ChangeEntityPosition(GridPosition startPosition, GridPosition targetPosition, bool teleport = false)
    {
        if (!TryGetEntity(startPosition, out var entity) || !IsTraversable(targetPosition)) return false;
        _entities[targetPosition.Dim1] = entity;
        _entities[startPosition.Dim1] = null;
        entity.SetPosition(targetPosition);
        _moves.Add(new EntityMove(entity.Uid, startPosition, targetPosition, teleport));
        ApplyTerrainAt(targetPosition);
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
        return position.IsValid() && _entities[position.Dim1] == null;
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
        for (var i = 0; i < _terrain.Length; i++)
        {
            hash.Add((int)_terrain[i]);
            hash.Add(_terrainTurns[i]);
        }
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

        var hasTraits = entity.TryGetComponent<TraitComponent>(out var traits);
        hash.Add(hasTraits);
        if (hasTraits)
        {
            hash.Add(traits.Statuses.Count);
            foreach (var status in traits.Statuses)
            {
                hash.Add(status.Id);
                hash.Add(status.Count);
            }
        }

        var hasAbilities = entity.TryGetComponent<AbilityComponent>(out var abilities);
        hash.Add(hasAbilities);
        if (!hasAbilities) return;
        hash.Add(abilities.List.Count);
        foreach (var ability in abilities.List)
        {
            hash.Add(ability.Id);
            hash.Add(ability.Cooldown);
            hash.Add(ability.CastsThisTurn);
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
