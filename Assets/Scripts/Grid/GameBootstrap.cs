using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameBootstrap : LoggableBehaviour
{
    private enum StartMode { Local, Steam }

    private const string TestTeamId = "TestTeam";

    private static bool _contentRegistered;

    [SerializeField] private GridManager _gridManager;
    [SerializeField] private List<EntityAssets> _entityAssets;
    [SerializeField] private List<StatusAssets> _statusAssets;
    [SerializeField] private List<AbilityVisuals> _abilityVisuals;
    [SerializeField] private List<TerrainAssets> _terrainAssets;
    [SerializeField] private bool _autoStart = true;
    [SerializeField] private string _mapId = "8x8";
    [SerializeField] private string _team1Id = TestTeamId;
    [SerializeField] private string _team2Id = TestTeamId;
    [SerializeField] private StartMode _startMode = StartMode.Local;

    private void Start()
    {
        if (!_autoStart) return;

        RegisterContent();
        if (!IdRegistry<GridDefinition>.TryGet(_mapId, out var definition))
        {
            LogError($"{_mapId} not found");
            return;
        }

        CreateTestTeam();
        var saved1 = JsonHandler.LoadData<SavedTeam>(_team1Id);
        var saved2 = JsonHandler.LoadData<SavedTeam>(_team2Id);
        foreach (var saved in new[] { saved1, saved2 })
            if (saved.Validate(definition) is { } savedIssue) LogError($"Team '{saved.Id}': {savedIssue}");
        var team1 = saved1.ForSeat(1);
        var team2 = saved2.ForSeat(2);
        var teams = new[] { team1, team2 };

        ValidateContent(definition, teams);

        if (_startMode == StartMode.Steam)
        {
            gameObject.AddComponent<SteamMatchController>().Init(_gridManager, definition, team1, team2);
            return;
        }

        var setup = new MatchSetup(definition.Id, new[] { "local-1", "local-2" }, teams);
        var issue = setup.Validate(definition);
        if (issue != null)
        {
            LogError($"Match setup invalid: {issue}");
            return;
        }
        _gridManager.StartGame(definition, setup.SeedCommands());
    }

    private void ValidateContent(GridDefinition map, IReadOnlyList<TeamData> teams)
    {
        foreach (var issue in RegistryValidator.Validate(map, teams)) LogError(issue);

        var spawnable = new HashSet<string>();
        foreach (var team in teams)
        foreach (var unit in team.UnitStartPositions) spawnable.Add(unit.UnitId);
        if (map.EntityStartPositions != null) foreach (var spec in map.EntityStartPositions) spawnable.Add(spec.EntityId);
        foreach (var id in spawnable)
        {
            if (!IdRegistry<EntityConfig>.TryGet(id, out var entityConfig)) continue;
            if (!IdRegistry<EntityAssets>.TryGet(id, out _))
                LogWarning($"Entity '{id}' has no EntityAssets (no model/sprite)");
            foreach (var abilityId in entityConfig.Abilities)
                if (!IdRegistry<AbilityVisuals>.TryGet(abilityId, out _)) LogError($"Ability '{abilityId}' on '{id}' has no AbilityVisuals (no icon)");
        }
    }

    private void Update()
    {
        if (_debug && _gridManager.IsGameStarted
            && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            var state = _gridManager.GridState;
            _gridManager.Turns.EndTurn();
            Log($"[debug] end turn -> active={state.ActivePlayer}, turn={state.Turn}, mana={state.GetMana(state.ActivePlayer)}");
        }
    }

    public void RegisterContent()
    {
        if (_contentRegistered) return;
        _contentRegistered = true;
        ContentRegistry.RegisterAll();
        RegistryManager.Register(_entityAssets);
        RegistryManager.Register(_statusAssets);
        RegistryManager.Register(_abilityVisuals);
        RegistryManager.Register(_terrainAssets);
    }

    private static void CreateTestTeam()
    {
        if (JsonHandler.Exists(TestTeamId)) return;
        JsonHandler.SaveData(new SavedTeam(TestTeamId, "8x8", new List<SavedUnit>
        {
            new("Rogue", new[] { 9, 54 }),
            new("Fire Mage", new[] { 11, 52 }),
            new("Barbarian", new[] { 12, 51 }),
            new("Knight", new[] { 14, 49 })
        }));
    }
}
