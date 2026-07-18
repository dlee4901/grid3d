using System.Collections.Generic;
using System.IO;
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

        InitTestRegistry();
        if (!IdRegistry<GridDefinition>.TryGet(_mapId, out var definition))
        {
            LogError($"{_mapId} not found");
            return;
        }
        _gridManager.StartGame(definition, BuildTeamSeedCommands());
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

    private void InitTestRegistry()
    {
        IdRegistry<EntityConfig>.Register(UnitConfigs.Mage);
        IdRegistry<AbilityConfig>.Register(AbilityConfigs.IcicleBlast);
        IdRegistry<AbilityConfig>.Register(AbilityConfigs.MoveStraight3Step);
        RegistryManager.Register(_entityAssets);

        var mapsPath = Path.Combine(Application.streamingAssetsPath, "Content/Maps");
        RegistryManager.LoadAndRegister<GridDefinition>(mapsPath);
    }

    private List<ICommand> BuildTeamSeedCommands()
    {
        CreateTestTeams();
        var team1 = JsonHandler.LoadData<TeamData>("TestTeam1");
        var team2 = JsonHandler.LoadData<TeamData>("TestTeam2");
        return new List<ICommand>
        {
            new LoadTeamCommand(1, team1),
            new LoadTeamCommand(2, team2),
        };
    }

    private void CreateTestTeams()
    {
        var team1 = new TeamData("TestTeam1", "TestMap1", new Dictionary<int, string>{ {2, "Mage"}, {5, "Mage"}, {10, "Mage"} });
        var team2 = new TeamData("TestTeam2", "TestMap1", new Dictionary<int, string>{ {110, "Mage"} });
        JsonHandler.SaveData(team1);
        JsonHandler.SaveData(team2);
    }
}
