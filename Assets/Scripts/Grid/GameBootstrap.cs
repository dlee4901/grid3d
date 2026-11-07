using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameBootstrap : LoggableBehaviour
{
    private enum StartMode { Local, Steam }

    [SerializeField] private GridManager _gridManager;
    [SerializeField] private List<EntityAssets> _entityAssets;
    [SerializeField] private bool _autoStart = true;
    [SerializeField] private string _mapId = "TestMap1";
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

        CreateTestTeams();
        var team1 = JsonHandler.LoadData<TeamData>("TestTeam1");
        var team2 = JsonHandler.LoadData<TeamData>("TestTeam2");
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
            if (IdRegistry<EntityConfig>.TryGet(id, out _) && !IdRegistry<EntityAssets>.TryGet(id, out _))
                LogWarning($"Entity '{id}' has no EntityAssets (no model/sprite)");
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

    private void RegisterContent()
    {
        ContentRegistry.RegisterAll();
        RegistryManager.Register(_entityAssets);
    }

    private void CreateTestTeams()
    {
        var team1 = new TeamData("TestTeam1", "TestMap1", new List<(int, string)> { (9, "Rogue"), (11, "Fire Mage"), (12, "Barbarian"), (14, "Knight") });
        var team2 = new TeamData("TestTeam2", "TestMap1", new List<(int, string)> { (54, "Rogue"), (52, "Fire Mage"), (51, "Barbarian"), (49, "Knight") });
        JsonHandler.SaveData(team1);
        JsonHandler.SaveData(team2);
    }
}
