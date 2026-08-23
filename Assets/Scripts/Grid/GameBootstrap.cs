using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameBootstrap : LoggableBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private List<EntityAssets> _entityAssets;
    [SerializeField] private bool _autoStart = true;
    [SerializeField] private string _mapId = "TestMap1";

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

        var seed = new List<ICommand>
        {
            new LoadTeamCommand(1, team1), 
            new LoadTeamCommand(2, team2), 
            new SpawnTeamsCommand()
        };
        _gridManager.StartGame(definition, seed);
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
            _gridManager.Submit(new EndTurnCommand(state.ActivePlayer));
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
        var team1 = new TeamData("TestTeam1", "TestMap1", new List<(int, string)> { (15, "Flameweaver"), (17, "Flameweaver") });//{2, "Barbarian"}, {5, "Rogue"}, {10, "Knight"} });
        var team2 = new TeamData("TestTeam2", "TestMap1", new List<(int, string)> { (117, "Flameweaver"), (119, "Flameweaver") });//{110, "Barbarian"}, {113, "Rogue"}, {116, "Knight"} });
        JsonHandler.SaveData(team1);
        JsonHandler.SaveData(team2);
    }
}
