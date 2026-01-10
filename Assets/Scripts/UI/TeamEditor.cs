using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamEditor : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _mapDropdown;
    [SerializeField] private RenamableDropdown _teamDropdown;
    [SerializeField] private GridManager _gridManager;

    // TODO: Unit List UI Manager
    [SerializeField] private GridLayoutGroup _unitListContainer;

    private List<UnitUIManager> _unitsUI;
    private PlayerData _playerData;
    private Dictionary<int, List<TeamData>> _teams;

    private void Start()
    {
        _mapDropdown.onValueChanged.AddListener(OnMapDropdownValueChanged);
        //_gridManager.LoadMap();
        //InitUnits();
        //LoadPlayerData();
    }

    private void OnMapDropdownValueChanged(int value)
    {
        //_teams = _playerData.GetTeams(_mapDropdown.value + 1);
        //_teamDropdown.SetOptions(_playerData.GetTeamNames());
    }

    private void SaveTeamData()
    {
       // TeamData teamData = new()
    }

    private void InitPlayerData()
    {
        _playerData = EngineUtil.LoadJsonData<PlayerData>();
        if (_playerData == null) return;
        // foreach (TeamData team in _playerData.GetTeams())
        // {

        // }
    }

    private void LoadPlayerData()
    {
        PlayerData playerData = EngineUtil.LoadJsonData<PlayerData>();
        if (playerData != null)
        {
            List<TeamData> teams = playerData.Teams;
        }
    }

    // private void InitUnits()
    // {
    //     _unitsUI = new List<UnitUIManager>();
    //     Debug.Log("InitUnits");
    //     foreach (UnitManager unit in UnitList.Singleton.Units)
    //     {
    //         UnitUIManager unitUI = EngineUtil.CreateUIGameObject<UnitUIManager>();
    //         unitUI.Init(unit.Unit.Id, unit.GetSprite(), _unitListContainer.transform);
    //         _unitsUI.Add(unitUI);
    //         Debug.Log(unit.Unit.Id);
    //     }
    // }
}