using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TeamBuilder : LoggableBehaviour
{
    private sealed class DraftUnit
    {
        public string UnitId;
        public int[] Positions;
    }

    private const string NewTeamName = "New Team";
    private const string NewTeamOption = "<align=center>+</align>";

    [SerializeField] private GameBootstrap _bootstrap;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private TMP_Dropdown _mapDropdown;
    [SerializeField] private DeletableDropdown _teamDropdown;
    [SerializeField] private TMP_InputField _nameField;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private UnitFilterView _filter;
    [SerializeField] private RectTransform _unitGrid;
    [SerializeField] private SquarePortrait _unitCardPrefab;
    [SerializeField] private PortraitElements _unitCardElements = PortraitElements.Cost;
    [SerializeField] private float _unitCardHoldTime;
    [SerializeField] private DropShadow.Direction _unitCardShadow = DropShadow.Direction.Right;
    [SerializeField] private ScrollRect _unitScroll;
    [SerializeField] private EntityView _entityView;
    [SerializeField] private ResolutionOrderView _resolutionOrder;
    [SerializeField] private float _dragLift = 0.5f;
    [SerializeField] private float _holdTime = 0.25f;

    private static string _mapId = "8x8";
    private static string _teamId;

    private GridDefinition _map;
    private readonly List<DraftUnit> _units = new();
    private int _side = 1;
    private string _armedUnitId;
    private DraftUnit _selected;
    private DraftUnit _pressedUnit;
    private int _pressedPosition;
    private float _pressedAt;
    private bool _dragging;
    private bool _draggingFromCard;
    private GameObject _ghost;
    private float _ghostHeight;
    private EntityConfig _cardConfig;
    private Vector2 _cardPressPoint;
    private bool _cardMoved;
    private DraftUnit _selectedBeforeCard;
    private string _armedBeforeCard;
    private bool _wasTargeting;
    private string _loadedId;

    private readonly List<SavedTeam> _teams = new();
    private readonly List<(EntityConfig Config, SquarePortrait Card)> _cards = new();

    private void Start()
    {
        _bootstrap.RegisterContent();
        if (!IdRegistry<GridDefinition>.TryGet(_mapId, out _map))
        {
            LogError($"Map '{_mapId}' not found");
            return;
        }
        _gridManager.StartGame(_map, Seeds());
        _gridManager.Input.OnPositionPressed += PressBoard;
        _gridManager.Input.OnPositionSelected += ReleaseOnBoard;
        _gridManager.Input.OnReleasedOffGrid += ReleaseOffBoard;
        _gridManager.Input.OnCancelClicked += CancelClick;
        _gridManager.Player.TargetingChanged += RefreshHighlights;
        _resolutionOrder.Reordered += Reorder;
        BindPanel();
        var teams = TeamsForMap();
        var team = teams.Find(t => t.Id == _teamId) ?? (teams.Count > 0 ? teams[0] : null);
        if (team != null) Open(team);
        else NewTeam();
    }

    private void OnDestroy()
    {
        if (_gridManager == null || _gridManager.Input == null) return;
        _gridManager.Input.OnPositionPressed -= PressBoard;
        _gridManager.Input.OnPositionSelected -= ReleaseOnBoard;
        _gridManager.Input.OnReleasedOffGrid -= ReleaseOffBoard;
        _gridManager.Input.OnCancelClicked -= CancelClick;
        _gridManager.Player.TargetingChanged -= RefreshHighlights;
        if (_resolutionOrder != null) _resolutionOrder.Reordered -= Reorder;
    }

    private List<Command> Seeds() => new()
    {
        new LoadTeamCommand(_side, Draft(null).ForSeat(_side)),
        new SpawnTeamsCommand()
    };

    private SavedTeam Draft(string id)
    {
        var units = new List<SavedUnit>();
        foreach (var unit in _units) units.Add(new SavedUnit(unit.UnitId, (int[])unit.Positions.Clone()));
        return new SavedTeam(id, _map.Id, units);
    }

    private void Update()
    {
        _wasTargeting = _gridManager.Player.State is AbilityInputState;
        if (_pressedUnit != null && !_dragging && Pointer.current.press.isPressed
            && (Time.unscaledTime - _pressedAt >= _holdTime || PointerPosition() != _pressedPosition))
            StartDrag(_pressedUnit, null, false);
        if (!_dragging) return;
        MoveGhost();
        if (_draggingFromCard && Pointer.current.press.wasReleasedThisFrame) ReleaseCardDrag();
    }

    private void PressBoard(GridSource cell)
    {
        _pressedPosition = cell.Position.Dim1;
        _pressedUnit = _wasTargeting ? null : UnitAt(_pressedPosition);
        _pressedAt = Time.unscaledTime;
    }

    private void ReleaseOnBoard(GridSource cell)
    {
        var position = cell.Position.Dim1;
        var pressed = _pressedUnit;
        _pressedUnit = null;
        if (_gridManager.Player.State is AbilityInputState) return;
        if (pressed != null && (_dragging || position != _pressedPosition))
        {
            EndDrag();
            if (IsOpenSpawn(position)) Move(pressed, position);
            else Deselect();
            return;
        }
        if (!_wasTargeting && IsLegal(position))
        {
            Commit(position);
            return;
        }
        _armedUnitId = null;
        _selected = _gridManager.Player.CurrentSelection is { Entity: not null } source ? UnitAt(source.Position.Dim1) : null;
        RefreshHighlights();
        RefreshPanel();
    }

    private void ReleaseOffBoard()
    {
        if (_wasTargeting) return;
        var pressed = _pressedUnit;
        _pressedUnit = null;
        EndDrag();
        if (pressed == null)
        {
            Deselect();
            return;
        }
        _units.Remove(pressed);
        _selected = null;
        Save();
        Rebuild();
    }

    private void PressCard(EntityConfig config)
    {
        if (PlacedUnit(config.Id) is { } placed)
        {
            if (placed == _selected) Deselect();
            else Select(placed);
            return;
        }
        _armedUnitId = _armedUnitId == config.Id ? null : config.Id;
        _selected = null;
        Reselect();
    }

    private void CancelClick()
    {
        if (!_wasTargeting) Deselect();
    }

    private void HoldCard(EntityConfig config)
    {
        _cardConfig = config;
        _cardPressPoint = Pointer.current.position.ReadValue();
        _cardMoved = false;
        _selectedBeforeCard = _selected;
        _armedBeforeCard = _armedUnitId;
        var placed = PlacedUnit(config.Id);
        StartDrag(placed, placed == null ? config.Id : null, true);
    }

    private GridDirectionFacing GhostFacing(DraftUnit unit, string armedUnitId)
    {
        var state = _gridManager.GridState;
        if (unit != null && state.GetEntity(new GridPosition(state, unit.Positions[_side - 1])) is { } placed) return placed.Facing;
        IdRegistry<EntityConfig>.TryGet(unit?.UnitId ?? armedUnitId, out var config);
        return Entity.Create(config, _side).Facing;
    }

    private void StartDrag(DraftUnit unit, string armedUnitId, bool fromCard)
    {
        _selected = unit;
        _armedUnitId = armedUnitId;
        _dragging = true;
        _draggingFromCard = fromCard;
        _unitScroll.enabled = !fromCard;
        IdRegistry<EntityAssets>.TryGet(unit?.UnitId ?? armedUnitId, out var assets);
        _ghost = Instantiate(assets.Model3D, Vector3.zero, EntityRenderer.RotationOf(GhostFacing(unit, armedUnitId)));
        _ghostHeight = EntityRenderer.ModelHeight(_ghost);
        _ghost.SetActive(!fromCard);
        if (unit != null) _gridManager.Renderer.Entities.HideModel(new GridPosition(_gridManager.GridState, unit.Positions[_side - 1]));
        Reselect();
        MoveGhost();
    }

    private void ReleaseCardDrag()
    {
        var cell = _gridManager.Input.CellUnderPointer();
        var clicked = !_cardMoved;
        EndDrag();
        if (clicked)
        {
            _selected = _selectedBeforeCard;
            _armedUnitId = _armedBeforeCard;
            PressCard(_cardConfig);
            return;
        }
        if (cell is { } target && IsLegal(target.Position.Dim1)) Commit(target.Position.Dim1);
        else Deselect();
    }

    private void EndDrag()
    {
        _dragging = false;
        _draggingFromCard = false;
        _unitScroll.enabled = true;
        if (_ghost != null) Destroy(_ghost);
        _gridManager.Renderer.Entities.HideModel(null);
    }

    private void MoveGhost()
    {
        if (_draggingFromCard && !_cardMoved)
        {
            _cardMoved = Vector2.Distance(Pointer.current.position.ReadValue(), _cardPressPoint) > EventSystem.current.pixelDragThreshold;
            _ghost.SetActive(_cardMoved);
        }
        var renderer = _gridManager.Renderer;
        if (_gridManager.Input.CellUnderPointer() is { } cell && IsLegal(cell.Position.Dim1))
        {
            _ghost.transform.position = renderer.CellCenter(cell.Position);
            return;
        }
        var head = renderer.CellCenter(new GridPosition(_gridManager.GridState, 0), _dragLift + _ghostHeight);
        var ray = _gridManager.MainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());
        if (!new Plane(Vector3.up, head).Raycast(ray, out var distance)) return;
        _ghost.transform.position = ray.GetPoint(distance) + Vector3.down * _ghostHeight;
    }

    private int PointerPosition() => _gridManager.Input.CellUnderPointer() is { } cell ? cell.Position.Dim1 : SavedUnit.Unplaced;

    private void Select(DraftUnit unit)
    {
        _selected = unit;
        _armedUnitId = null;
        Reselect();
    }

    private void Deselect()
    {
        _selected = null;
        _armedUnitId = null;
        Reselect();
    }

    private bool IsLegal(int position)
    {
        if (!IsOpenSpawn(position)) return false;
        if (_selected != null) return true;
        if (_armedUnitId == null) return false;
        return _units.Exists(u => u.UnitId == _armedUnitId) || Cost() + CostOf(_armedUnitId) <= _map.MaxTeamCost;
    }

    private void Commit(int position)
    {
        if (_selected != null)
        {
            Move(_selected, position);
            return;
        }
        var unit = _units.Find(u => u.UnitId == _armedUnitId);
        if (unit == null)
        {
            unit = new DraftUnit { UnitId = _armedUnitId, Positions = new int[_map.PlayerCount] };
            for (var i = 0; i < unit.Positions.Length; i++) unit.Positions[i] = SavedUnit.Unplaced;
            _units.Add(unit);
        }
        _armedUnitId = null;
        Move(unit, position);
    }

    private void Move(DraftUnit unit, int position)
    {
        unit.Positions[_side - 1] = position;
        if (_map.IsMirrored) unit.Positions[2 - _side] = _map.Mirror(position);
        _selected = null;
        Save();
        Rebuild();
    }

    private void ToggleSide()
    {
        _side = 3 - _side;
        _selected = null;
        _armedUnitId = null;
        Rebuild();
    }

    private void Save() => JsonHandler.SaveData(Draft(_loadedId));

    private void BeginRename()
    {
        _teamDropdown.interactable = false;
        _teamDropdown.captionText.gameObject.SetActive(false);
        _nameField.gameObject.SetActive(true);
        _nameField.SetTextWithoutNotify(_loadedId);
        _nameField.Select();
        _nameField.ActivateInputField();
    }

    private void EndRename(string text)
    {
        if (!_nameField.gameObject.activeSelf) return;
        _nameField.gameObject.SetActive(false);
        _teamDropdown.captionText.gameObject.SetActive(true);
        _teamDropdown.interactable = true;
        var name = text.Trim();
        if (name == _loadedId) return;
        if (NameIssue(name) is { } issue)
        {
            LogWarning(issue);
            return;
        }
        JsonHandler.SaveData(Draft(name));
        if (!string.Equals(name, _loadedId, StringComparison.OrdinalIgnoreCase)) JsonHandler.Delete(_loadedId);
        _loadedId = name;
        _teamId = name;
        LoadTeams();
        RefreshTeamOptions();
    }

    private string NameIssue(string name)
    {
        if (name.Length == 0) return "Team name is empty";
        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return "Team name has an invalid character";
        if (!string.Equals(name, _loadedId, StringComparison.OrdinalIgnoreCase) && JsonHandler.Exists(name)) return $"A team named {name} already exists";
        return null;
    }

    private void Open(SavedTeam team)
    {
        _units.Clear();
        foreach (var saved in team.Units)
        {
            var positions = new int[_map.PlayerCount];
            for (var seat = 1; seat <= positions.Length; seat++) positions[seat - 1] = saved.PositionFor(seat);
            _units.Add(new DraftUnit { UnitId = saved.UnitId, Positions = positions });
        }
        _loadedId = team.Id;
        _teamId = team.Id;
        _selected = null;
        _armedUnitId = null;
        LoadTeams();
        Rebuild();
    }

    private void NewTeam()
    {
        var name = NewTeamName;
        for (var i = 2; JsonHandler.Exists(name); i++) name = $"{NewTeamName} {i}";
        var team = new SavedTeam(name, _map.Id, new List<SavedUnit>());
        JsonHandler.SaveData(team);
        Open(team);
    }

    private bool CanAddTeam => _units.Count > 0;
    private int TeamOffset => CanAddTeam ? 1 : 0;

    private void SelectTeam(int index)
    {
        if (index < TeamOffset) NewTeam();
        else Open(_teams[index - TeamOffset]);
    }

    private void DeleteTeam(int index)
    {
        var team = _teams[index - TeamOffset];
        JsonHandler.Delete(team.Id);
        if (team.Id != _loadedId)
        {
            LoadTeams();
            RefreshTeamOptions();
            return;
        }
        _loadedId = null;
        LoadTeams();
        if (_teams.Count > 0) Open(_teams[0]);
        else NewTeam();
    }

    private void LoadTeams()
    {
        DeleteEmptyTeams();
        _teams.Clear();
        _teams.AddRange(TeamsForMap());
    }

    private void DeleteEmptyTeams()
    {
        foreach (var id in JsonHandler.Ids())
            if (!string.Equals(id, _loadedId, StringComparison.OrdinalIgnoreCase) && JsonHandler.LoadData<SavedTeam>(id).Units.Count == 0)
                JsonHandler.Delete(id);
    }

    private void RefreshTeamOptions()
    {
        var options = new List<string>();
        if (CanAddTeam) options.Add(NewTeamOption);
        foreach (var team in _teams) options.Add(team.Id);
        _teamDropdown.ClearOptions();
        _teamDropdown.AddOptions(options);
        _teamDropdown.SetValueWithoutNotify(_teams.FindIndex(t => t.Id == _loadedId) + TeamOffset);
    }

    private void SelectMap(string mapId)
    {
        if (mapId == _map.Id) return;
        _mapId = mapId;
        _teamId = null;
        SceneManager.LoadScene(gameObject.scene.name);
    }

    private List<SavedTeam> TeamsForMap()
    {
        var teams = new List<SavedTeam>();
        foreach (var id in JsonHandler.Ids())
        {
            var team = JsonHandler.LoadData<SavedTeam>(id);
            if (team.MapId == _map.Id) teams.Add(team);
        }
        teams.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.OrdinalIgnoreCase));
        return teams;
    }

    private void Rebuild()
    {
        _gridManager.Restart(Seeds());
        Reselect();
    }

    private void Reselect()
    {
        var player = _gridManager.Player;
        player.ResetToIdle();
        if (_selected != null)
        {
            var state = _gridManager.GridState;
            var gridPosition = new GridPosition(state, _selected.Positions[_side - 1]);
            player.OnPositionSelected(new GridSource(state, gridPosition, state.GetEntity(gridPosition)));
        }
        else if (_armedUnitId != null && IdRegistry<EntityConfig>.TryGet(_armedUnitId, out var config)) _entityView.ShowDetached(Entity.Create(config, _side));
        else _entityView.ShowDetached(null);
        RefreshHighlights();
        RefreshPanel();
    }

    private void RefreshHighlights()
    {
        if (_gridManager.Player.State is AbilityInputState) return;
        var highlights = _gridManager.Renderer.Highlights;
        highlights.Clear();
        var selected = _gridManager.Player.CurrentSelection?.Position.Dim1;
        var starts = new HashSet<GridPosition>();
        var state = _gridManager.GridState;
        for (var i = 0; i < state.Size; i++)
            if (IsSpawn(i) && i != selected) starts.Add(new GridPosition(state, i));
        highlights.Highlight(starts, HighlightType.StartingPositions);
    }

    private DraftUnit PlacedUnit(string unitId) => _units.Find(u => u.UnitId == unitId && u.Positions[_side - 1] != SavedUnit.Unplaced);

    private void Reorder(IReadOnlyList<int> uids)
    {
        var state = _gridManager.GridState;
        var ordered = new List<DraftUnit>();
        foreach (var uid in uids)
            if (state.GetEntityByUid(uid) is { } entity && UnitAt(entity.Position.Dim1) is { } unit) ordered.Add(unit);
        foreach (var unit in _units)
            if (!ordered.Contains(unit)) ordered.Add(unit);
        _units.Clear();
        _units.AddRange(ordered);
        Save();
        Rebuild();
    }

    private DraftUnit UnitAt(int position) => _units.Find(u => u.Positions[_side - 1] == position);

    private bool IsSpawn(int position)
    {
        var gridPosition = new GridPosition(_map.Shape, position);
        return gridPosition.IsValid() && _map.GetSpawnPlayer(gridPosition) == _side;
    }

    private bool IsOpenSpawn(int position) => IsSpawn(position) && UnitAt(position) == null;

    private int Cost()
    {
        var cost = 0;
        foreach (var unit in _units) cost += CostOf(unit.UnitId);
        return cost;
    }

    private static int CostOf(string unitId) => IdRegistry<EntityConfig>.TryGet(unitId, out var config) ? config.Cost : 0;

    private static IEnumerable<GridDefinition> Maps()
    {
        foreach (var field in typeof(MapConfigs).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            if (field.GetValue(null) is GridDefinition map) yield return map;
    }

    private void BindPanel()
    {
        var maps = new List<GridDefinition>(Maps());
        maps.Sort((a, b) => a.Size.CompareTo(b.Size));
        _mapDropdown.ClearOptions();
        _mapDropdown.AddOptions(maps.ConvertAll(map => map.Id));
        _mapDropdown.SetValueWithoutNotify(maps.FindIndex(map => map.Id == _map.Id));
        _mapDropdown.onValueChanged.AddListener(index => SelectMap(maps[index].Id));
        _teamDropdown.onValueChanged.AddListener(SelectTeam);
        _teamDropdown.GetComponentInChildren<InteractableUI>().OnHoldTriggered += BeginRename;
        _nameField.onEndEdit.AddListener(EndRename);
        _teamDropdown.DeleteRequested += DeleteTeam;
        _teamDropdown.CanDelete = index => index >= TeamOffset;
        _filter.Changed += ApplyFilter;
        BuildUnitGrid();
    }

    private void BuildUnitGrid()
    {
        var configs = new List<EntityConfig>();
        foreach (var field in typeof(UnitConfigs).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            if (field.GetValue(null) is EntityConfig { Cost: > 0 } config && IdRegistry<EntityAssets>.TryGet(config.Id, out _)) configs.Add(config);
        configs.Sort((a, b) => a.Cost != b.Cost ? a.Cost.CompareTo(b.Cost) : string.Compare(a.Id, b.Id, StringComparison.Ordinal));
        foreach (var config in configs)
        {
            IdRegistry<EntityAssets>.TryGet(config.Id, out var assets);
            var card = Instantiate(_unitCardPrefab, _unitGrid);
            card.Configure(_unitCardElements);
            card.SetShadowDirection(_unitCardShadow);
            card.Button.HoldTime = _unitCardHoldTime;
            card.Init(assets.Model2D);
            card.SetCost(config.Cost);
            var cardConfig = config;
            card.Button.OnClickCompleted += () => PressCard(cardConfig);
            card.Button.OnHoldTriggered += () => HoldCard(cardConfig);
            _cards.Add((config, card));
        }
    }

    private void ApplyFilter()
    {
        foreach (var (config, card) in _cards) card.gameObject.SetActive(_filter.Matches(config.Id, config.Cost));
    }

    private void RefreshPanel()
    {
        _costText.text = $"Cost {Cost()} / {_map.MaxTeamCost}";
        foreach (var (config, card) in _cards)
        {
            card.SetArmed(config.Id == _armedUnitId);
            card.SetPlaced(PlacedUnit(config.Id) != null);
        }
        RefreshTeamOptions();
    }
}
