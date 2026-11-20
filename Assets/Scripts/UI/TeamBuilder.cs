using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TeamBuilder : LoggableBehaviour
{
    private sealed class DraftUnit
    {
        public string UnitId;
        public int[] Positions;
    }

    [SerializeField] private GameBootstrap _bootstrap;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private AbilityIcon _unitIconPrefab;
    [SerializeField] private InteractableUI _buttonPrefab;
    [SerializeField] private Material _labelMaterial;
    [SerializeField] private int _slotCount = 4;
    [SerializeField] private float _unitIconSize = 128f;
    [SerializeField] private float _rowHeight = 64f;
    [SerializeField] private float _labelHeight = 40f;
    [SerializeField] private float _spacing = 12f;
    [SerializeField] private int _fontSize = 32;
    [SerializeField] private Color32 _selectedLabelColor = new(255, 192, 64, 255);
    [SerializeField] private Color32 _errorLabelColor = new(255, 128, 112, 255);

    private static string _mapId = "TestMap1";
    private static int _slot = 1;
    private static bool _loadSlotOnStart;

    private GridDefinition _map;
    private readonly List<DraftUnit> _units = new();
    private int _side = 1;
    private string _armedUnitId;
    private DraftUnit _selected;
    private int? _pendingPosition;

    private readonly Dictionary<string, AbilityIcon> _rosterIcons = new();
    private readonly List<TMP_Text> _slotLabels = new();
    private readonly Dictionary<string, TMP_Text> _mapLabels = new();
    private TMP_Text _costText;
    private TMP_Text _statusText;
    private TMP_Text _sideLabel;

    private void Start()
    {
        _bootstrap.RegisterContent();
        if (!IdRegistry<GridDefinition>.TryGet(_mapId, out _map))
        {
            LogError($"Map '{_mapId}' not found");
            return;
        }
        _gridManager.Input.OnPositionSelected += DecidePress;
        _gridManager.StartGame(_map, Seeds());
        _gridManager.Input.OnPositionSelected += ApplyPress;
        _gridManager.Player.TargetingChanged += RefreshHighlights;
        BuildPanel();
        if (_loadSlotOnStart)
        {
            _loadSlotOnStart = false;
            Load();
            return;
        }
        Rebuild();
    }

    private void OnDestroy()
    {
        if (_gridManager == null || _gridManager.Input == null) return;
        _gridManager.Input.OnPositionSelected -= DecidePress;
        _gridManager.Input.OnPositionSelected -= ApplyPress;
        _gridManager.Player.TargetingChanged -= RefreshHighlights;
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

    private void DecidePress(GridSource cell)
    {
        var position = cell.Position.Dim1;
        var occupant = UnitAt(position);
        _pendingPosition = null;
        if (occupant != null)
        {
            _selected = occupant;
            _armedUnitId = null;
        }
        else if (IsOpenSpawn(position) && (_armedUnitId != null || _selected != null)) _pendingPosition = position;
        else _selected = null;
        RefreshPanel();
    }

    private void ApplyPress(GridSource cell)
    {
        if (_pendingPosition is not { } position) return;
        _pendingPosition = null;
        if (_armedUnitId != null) Place(_armedUnitId, position);
        else Move(_selected, position);
    }

    private void Place(string unitId, int position)
    {
        var unit = _units.Find(u => u.UnitId == unitId && u.Positions[_side - 1] == SavedUnit.Unplaced);
        if (unit == null)
        {
            if (Cost() + CostOf(unitId) > _map.MaxTeamCost)
            {
                Status($"{unitId} would exceed the cost limit", true);
                return;
            }
            unit = new DraftUnit { UnitId = unitId, Positions = new int[_map.PlayerCount] };
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
        _selected = unit;
        Status(null);
        Rebuild();
    }

    private void Remove()
    {
        if (_selected == null) return;
        _units.Remove(_selected);
        _selected = null;
        Rebuild();
    }

    private void ToggleSide()
    {
        _side = 3 - _side;
        _selected = null;
        Rebuild();
    }

    private void Arm(string unitId)
    {
        _armedUnitId = _armedUnitId == unitId ? null : unitId;
        _selected = null;
        Rebuild();
    }

    private void SelectSlot(int slot)
    {
        _slot = slot;
        RefreshPanel();
    }

    private void Save()
    {
        var team = Draft(SlotId(_slot));
        if (team.Validate(_map) is { } issue)
        {
            Status(issue, true);
            return;
        }
        JsonHandler.SaveData(team);
        Status($"Saved team {_slot}");
    }

    private void Load()
    {
        var id = SlotId(_slot);
        if (!JsonHandler.Exists(id))
        {
            Status($"Team {_slot} is empty", true);
            Rebuild();
            return;
        }
        var team = JsonHandler.LoadData<SavedTeam>(id);
        if (team.MapId != _map.Id)
        {
            _mapId = team.MapId;
            _loadSlotOnStart = true;
            SceneManager.LoadScene(gameObject.scene.name);
            return;
        }
        _units.Clear();
        foreach (var saved in team.Units)
        {
            var positions = new int[_map.PlayerCount];
            for (var seat = 1; seat <= positions.Length; seat++) positions[seat - 1] = saved.PositionFor(seat);
            _units.Add(new DraftUnit { UnitId = saved.UnitId, Positions = positions });
        }
        _selected = null;
        _armedUnitId = null;
        Status($"Loaded team {_slot}");
        Rebuild();
    }

    private void SelectMap(string mapId)
    {
        if (mapId == _map.Id) return;
        _mapId = mapId;
        SceneManager.LoadScene(gameObject.scene.name);
    }

    private void Rebuild()
    {
        _gridManager.Restart(Seeds());
        if (_selected != null && _selected.Positions[_side - 1] is var position && position != SavedUnit.Unplaced)
        {
            var state = _gridManager.GridState;
            var gridPosition = new GridPosition(state, position);
            _gridManager.Player.OnPositionSelected(new GridSource(state, gridPosition, state.GetEntity(gridPosition)));
        }
        RefreshHighlights();
        RefreshPanel();
    }

    private void RefreshHighlights()
    {
        if (_armedUnitId == null && _selected == null) return;
        if (_gridManager.Player.State is not (IdleState or PositionSelectedState)) return;
        var open = new HashSet<GridPosition>();
        var state = _gridManager.GridState;
        for (var i = 0; i < state.Size; i++)
            if (IsOpenSpawn(i)) open.Add(new GridPosition(state, i));
        _gridManager.Renderer.Highlights.Highlight(open, HighlightType.AvailableTargets);
    }

    private DraftUnit UnitAt(int position) => _units.Find(u => u.Positions[_side - 1] == position);

    private bool IsOpenSpawn(int position)
    {
        var gridPosition = new GridPosition(_map.Shape, position);
        return gridPosition.IsValid() && _map.GetSpawnPlayer(gridPosition) == _side && UnitAt(position) == null;
    }

    private int Cost()
    {
        var cost = 0;
        foreach (var unit in _units) cost += CostOf(unit.UnitId);
        return cost;
    }

    private static int CostOf(string unitId) => IdRegistry<EntityConfig>.TryGet(unitId, out var config) ? config.Cost : 0;

    private static string SlotId(int slot) => $"Team{slot}";

    private void Status(string message, bool error = false)
    {
        if (_statusText == null) return;
        _statusText.text = message ?? "";
        _statusText.color = error ? _errorLabelColor : Color.white;
    }

    private void BuildPanel()
    {
        var layout = _panel.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = _spacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var units = new List<EntityConfig>();
        foreach (var field in typeof(UnitConfigs).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
            if (field.GetValue(null) is EntityConfig config && IdRegistry<EntityAssets>.TryGet(config.Id, out _)) units.Add(config);
        var columns = 2;
        var rows = (units.Count + columns - 1) / columns;
        var roster = Row(rows * _unitIconSize + (rows - 1) * _spacing, false);
        var grid = roster.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(_unitIconSize, _unitIconSize);
        grid.spacing = new Vector2(_spacing, _spacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.childAlignment = TextAnchor.UpperCenter;
        foreach (var config in units)
        {
            IdRegistry<EntityAssets>.TryGet(config.Id, out var assets);
            var icon = Instantiate(_unitIconPrefab, roster);
            icon.InitStatus(assets.Model2D, config.Cost);
            var unitId = config.Id;
            icon.OnClicked += _ => Arm(unitId);
            _rosterIcons[unitId] = icon;
        }

        _costText = Label(Row(_labelHeight, true));

        var edit = Row(_rowHeight, true);
        if (!_map.IsMirrored) _sideLabel = Button(edit, "", ToggleSide);
        Button(edit, "Remove", Remove);

        var slots = Row(_rowHeight, true);
        for (var slot = 1; slot <= _slotCount; slot++)
        {
            var index = slot;
            _slotLabels.Add(Button(slots, slot.ToString(), () => SelectSlot(index)));
        }

        var files = Row(_rowHeight, true);
        Button(files, "Save", Save);
        Button(files, "Load", Load);

        var maps = Row(_rowHeight, true);
        foreach (var field in typeof(MapConfigs).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
        {
            if (field.GetValue(null) is not GridDefinition map) continue;
            var mapId = map.Id;
            _mapLabels[mapId] = Button(maps, mapId, () => SelectMap(mapId));
        }

        _statusText = Label(Row(_labelHeight, true));
    }

    private void RefreshPanel()
    {
        foreach (var (unitId, icon) in _rosterIcons) icon.SetActiveVisual(unitId == _armedUnitId);
        if (_costText != null) _costText.text = $"Cost {Cost()} / {_map.MaxTeamCost}";
        if (_sideLabel != null) _sideLabel.text = $"Side {_side}";
        for (var i = 0; i < _slotLabels.Count; i++) _slotLabels[i].color = i + 1 == _slot ? _selectedLabelColor : Color.white;
        foreach (var (mapId, label) in _mapLabels) label.color = mapId == _map.Id ? _selectedLabelColor : Color.white;
    }

    private RectTransform Row(float height, bool horizontal)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(LayoutElement)).GetComponent<RectTransform>();
        row.SetParent(_panel, false);
        row.GetComponent<LayoutElement>().preferredHeight = height;
        if (!horizontal) return row;
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = _spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        return row;
    }

    private TMP_Text Button(RectTransform row, string text, System.Action onClick)
    {
        var button = Instantiate(_buttonPrefab, row);
        button.OnClickCompleted += onClick;
        var label = button.GetComponentInChildren<TMP_Text>();
        label.text = text;
        label.fontSize = _fontSize;
        return label;
    }

    private TMP_Text Label(RectTransform row)
    {
        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.rectTransform.SetParent(row, false);
        label.fontSharedMaterial = _labelMaterial;
        label.fontSize = _fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }
}
