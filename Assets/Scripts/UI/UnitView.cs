using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UnitView : StateView
{
    [SerializeField] private VerticalLayoutGroup _container;
    [SerializeField] private UnitLabel _unitLabelPrefab;
    [FormerlySerializedAs("_abilityIcon")] [SerializeField] private AbilityIcon _abilityIconPrefab;

    private readonly List<AbilityIcon> _icons = new();
    private UnitLabel _unitLabel;
    private int _labelUid;

    protected override bool ShowsPreview => false;

    protected override void Start()
    {
        base.Start();
        _gridManager.Player.SelectionChanged += OnInputChanged;
        _gridManager.Player.OnActiveAbilityChanged += OnActiveAbilityChanged;
        _gridManager.DisplayChanged += RefreshLabelHealth;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_gridManager == null) return;
        _gridManager.DisplayChanged -= RefreshLabelHealth;
        if (_gridManager.Player != null)
        {
            _gridManager.Player.SelectionChanged -= OnInputChanged;
            _gridManager.Player.OnActiveAbilityChanged -= OnActiveAbilityChanged;
        }
    }

    protected override void Refresh() => OnInputChanged(_gridManager.Player.CurrentSelection);

    private void OnActiveAbilityChanged(string activeId)
    {
        foreach (var icon in _icons) icon.SetActiveVisual(icon.AbilityId == activeId);
    }

    private void OnInputChanged(GridSource? source)
    {
        ClearMenu();
        if (source.HasValue) DisplayEntityInfo(source.Value);
    }
    
    // public void DisplayPositionInfo(Grid2D grid, int position)
    // {
    //     if (!grid.IsValidPosition(position)) 
    //         return;
    //     var entity = grid.GetEntity(position);
    //     if (entity != null)
    //         DisplayEntityInfo(entity);
    // }
    
    private void DisplayEntityInfo(GridSource source)
    {
        var entity = source.Entity;
        if (entity == null)
            return;
        if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var entityAssets))
            return;
        var unitLabel = Instantiate(_unitLabelPrefab, _container.transform);
        unitLabel.SetName(entity.Id);
        _unitLabel = unitLabel;
        _labelUid = entity.Uid;
        RefreshLabelHealth();
        
        if (!entity.TryGetComponent<AbilityComponent>(out var abilities))
            return;
        Log($"Selection info: {entity.Id}, abilities={abilities.List.Count}");
        var seat = _gridManager.LocalSeat;
        var actionable = source.Grid.IsAvailable(source.Position, seat);
        var abilityIcons = entityAssets.AbilityIcons;
        for (var i = 0; i < abilities.List.Count; i++)
        {
            if (abilityIcons.Count <= i)
                break;
            var icon = Instantiate(_abilityIconPrefab, _container.transform);
            icon.Init(abilityIcons[i], abilities.List[i], source);
            icon.OnPreviewRequested  += _gridManager.Player.OnAbilityPreview;
            icon.OnPreviewCancelled  += _gridManager.Player.OnAbilityCancelPreview;
            icon.OnActivateRequested += _gridManager.Player.OnAbilityActivate;
            icon.SetInteractable(actionable && source.Grid.CanCast(abilities.List[i], seat));
            _icons.Add(icon);
        }
        var activeId = _gridManager.Player.ActiveAbilityId;
        foreach (var icon in _icons) icon.SetActiveVisual(icon.AbilityId == activeId);
    }

    private void RefreshLabelHealth()
    {
        if (_unitLabel == null) return;
        var real = _gridManager.GridState.GetEntityByUid(_labelUid);
        if (real == null || !real.TryGetComponent<HealthComponent>(out var health)) return;
        var shown = _gridManager.DisplayedState.GetEntityByUid(_labelUid);
        var next = shown != null && shown.TryGetComponent<HealthComponent>(out var shownHealth) ? shownHealth.Current : 0;
        _unitLabel.HealthCounter.SetHealthCount(next);
        _unitLabel.HealthCounter.SetChange(next - health.Current);
    }

    private void ClearMenu()
    {
        _unitLabel = null;
        _icons.Clear();
        UnityUtil.DestroyAllChildren(_container.gameObject);
    }
    
    // private void EventManager_OnSelectUnit(object sender, EventManager.OnSelectUnitEventArgs e)
    // {
    //     DisplayUnitInfo(e.Unit);
    // }
    //
    // private void DisplayUnitInfo(UnitManager unit)
    // {
    //
    // }
}