using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EntityView : StateView
{
    [FormerlySerializedAs("_label")] [SerializeField] private RectTransform _container;
    [SerializeField] private UnitLabel _unitLabelPrefab;
    [FormerlySerializedAs("_abilityIcon")] [SerializeField] private AbilityIcon _abilityIconPrefab;
    [SerializeField] private RectTransform _statusContainer;
    [SerializeField] private AbilityIcon _statusIconPrefab;
    [SerializeField] private TooltipStack _tooltips;

    private readonly List<AbilityIcon> _icons = new();
    private UnitLabel _unitLabel;
    private AbilityIcon _tooltipIcon;
    private RectTransform _tooltipSource;
    private int _labelUid;
    private Entity _detached;

    protected override void Start()
    {
        base.Start();
        _gridManager.Player.SelectionChanged += OnInputChanged;
        _gridManager.Player.OnActiveAbilityChanged += OnActiveAbilityChanged;
        _gridManager.Player.TargetingChanged += RefreshAbilityTooltip;
        _gridManager.StateChanged += Rebuild;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_gridManager == null) return;
        _gridManager.StateChanged -= Rebuild;
        if (_gridManager.Player != null)
        {
            _gridManager.Player.SelectionChanged -= OnInputChanged;
            _gridManager.Player.OnActiveAbilityChanged -= OnActiveAbilityChanged;
            _gridManager.Player.TargetingChanged -= RefreshAbilityTooltip;
        }
    }

    private void Rebuild() => OnInputChanged(_gridManager.Player.CurrentSelection);

    public void ShowDetached(Entity entity)
    {
        ClearMenu();
        if (entity == null) return;
        _detached = entity;
        DisplayEntityInfo(new GridSource(_gridManager.GridState, default, entity));
    }

    protected override void Render(GridState shown)
    {
        if (_unitLabel == null) return;
        var real = _detached ?? _gridManager.GridState.GetEntityByUid(_labelUid);
        var entity = _detached ?? shown.GetEntityByUid(_labelUid);
        RenderHealth(real, entity);
        RenderAbilities(real, entity ?? real);
        RenderStatuses(entity ?? real);
    }

    private void OnActiveAbilityChanged(string activeId)
    {
        foreach (var icon in _icons) icon.SetActiveVisual(icon.AbilityId == activeId);
        RefreshAbilityTooltip();
    }

    private void RefreshAbilityTooltip()
    {
        if (_detached != null) return;
        var player = _gridManager.Player;
        var selected = player.State as AbilitySelectedState;
        var active = selected == null ? null : _icons.Find(icon => icon.AbilityId == selected.AbilityId);
        if (active != null)
        {
            if (!IsShowing(active)) ShowTooltip(active);
        }
        else if (!ReferenceEquals(_tooltipIcon, null) && _tooltipIcon.Ability is { IsCastable: true }) HideTooltip();
    }

    private void OnIconClicked(AbilityIcon icon)
    {
        if (icon.Ability is { IsCastable: true } && _detached == null) _gridManager.Player.OnAbilityActivate(icon.Ability, icon.Source);
        else if (IsShowing(icon)) HideTooltip();
        else ShowTooltip(icon);
    }

    private void ShowTooltip(AbilityIcon icon)
    {
        var state = _gridManager.GridState;
        var content = icon.Ability != null ? TooltipText.ForAbility(icon.Ability, state, icon.Source.Entity)
            : TooltipText.ForStatus(icon.Status, state);
        _tooltipSource = (RectTransform)icon.transform;
        _tooltips.OpenRoot(this, content, _tooltipSource, TooltipSide.Right);
        _tooltipIcon = icon;
    }

    private void HideTooltip()
    {
        _tooltips.Close(this);
        _tooltipIcon = null;
        _tooltipSource = null;
    }

    private bool IsShowing(AbilityIcon icon) => ReferenceEquals(_tooltipIcon, icon) && _tooltips.IsOpenFor(this, _tooltipSource);

    private void OnInputChanged(GridSource? source)
    {
        ClearMenu();
        if (source?.Entity != null) DisplayEntityInfo(source.Value);
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
        var unitLabel = Instantiate(_unitLabelPrefab, _container);
        unitLabel.Init(entity.Id, entityAssets.Model2D, IdRegistry<EntityConfig>.TryGet(entity.Id, out var config) ? config.Cost : 0);
        _unitLabel = unitLabel;
        _labelUid = entity.Uid;
        unitLabel.HealthCounter.SetOwner(entity, _gridManager.ViewerSeat);
        
        if (entity.TryGetComponent<AbilityComponent>(out var abilities))
            AddAbilityIcons(abilities, entityAssets, source);
        RenderShown();
    }

    private void AddAbilityIcons(AbilityComponent abilities, EntityAssets entityAssets, GridSource source)
    {
        Log($"Selection info: {source.Entity.Id}, abilities={abilities.List.Count}");
        var abilityIcons = entityAssets.AbilityIcons;
        for (var i = 0; i < abilities.List.Count; i++)
        {
            if (abilityIcons.Count <= i)
                break;
            var icon = Instantiate(_abilityIconPrefab, _container);
            icon.Init(abilityIcons[i], abilities.List[i], source);
            icon.OnClicked += OnIconClicked;
            _icons.Add(icon);
        }
        var activeId = _gridManager.Player.ActiveAbilityId;
        foreach (var icon in _icons) icon.SetActiveVisual(icon.AbilityId == activeId);
    }

    private void RenderAbilities(Entity real, Entity shown)
    {
        if (real == null || shown == null) return;
        if (!real.TryGetComponent<AbilityComponent>(out var realAbilities)
            || !shown.TryGetComponent<AbilityComponent>(out var shownAbilities)) return;
        var seat = _gridManager.LocalSeat;
        for (var i = 0; i < _icons.Count; i++)
            _icons[i].Render(shownAbilities.List[i], _detached != null || _gridManager.GridState.CanCastAt(real.Position, realAbilities.List[i], seat));
    }

    private void RenderStatuses(Entity shown)
    {
        UnityUtil.DestroyAllChildren(_statusContainer.gameObject);
        if (shown == null || !shown.TryGetComponent<StatusComponent>(out var statuses)) return;
        foreach (var status in statuses.Statuses)
        {
            if (!IdRegistry<StatusAssets>.TryGet(status.Id, out var statusAssets)) continue;
            var icon = Instantiate(_statusIconPrefab, _statusContainer);
            icon.InitStatus(statusAssets.Icon, status.Count, status);
            icon.OnClicked += OnIconClicked;
        }
    }

    private void RenderHealth(Entity real, Entity shown)
    {
        if (real == null || !real.TryGetComponent<HealthComponent>(out var health)) return;
        var next = shown != null && shown.TryGetComponent<HealthComponent>(out var shownHealth) ? shownHealth.Current : 0;
        _unitLabel.HealthCounter.SetHealthCount(next);
        _unitLabel.HealthCounter.SetPrevious(health.Current);
        _unitLabel.HealthCounter.SetDead(shown == null);
    }

    private void ClearMenu()
    {
        HideTooltip();
        if (_unitLabel != null) Destroy(_unitLabel.gameObject);
        foreach (var icon in _icons) Destroy(icon.gameObject);
        _unitLabel = null;
        _detached = null;
        _icons.Clear();
        UnityUtil.DestroyAllChildren(_statusContainer.gameObject);
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