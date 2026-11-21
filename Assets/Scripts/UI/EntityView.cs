using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class EntityView : StateView
{
    [FormerlySerializedAs("_label")] [SerializeField] private RectTransform _container;
    [SerializeField] private GameObject _unit;
    [SerializeField] private TMP_Text _unitName;
    [SerializeField] private SquarePortrait _portrait;
    [FormerlySerializedAs("_abilityIcon")] [SerializeField] private SquarePortrait _abilityIconPrefab;
    [SerializeField] private PortraitElements _abilityElements = PortraitElements.Mana | PortraitElements.Cooldown;
    [SerializeField] private RectTransform _statusContainer;
    [SerializeField] private SquarePortrait _statusIconPrefab;
    [SerializeField] private PortraitElements _statusElements = PortraitElements.Counter;
    [SerializeField] private Color _disabledTint = new(1f, 1f, 1f, 0.4f);
    [SerializeField] private TooltipStack _tooltips;

    private sealed class Icon
    {
        public SquarePortrait Portrait;
        public Ability Ability;
        public Status Status;
        public GridSource Source;
    }

    private readonly List<Icon> _icons = new();
    private Icon _tooltipIcon;
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
        if (!_unit.activeSelf) return;
        var real = _detached ?? _gridManager.GridState.GetEntityByUid(_labelUid);
        var entity = _detached ?? shown.GetEntityByUid(_labelUid);
        RenderHealth(real, entity);
        RenderAbilities(real, entity ?? real);
        RenderStatuses(entity ?? real);
    }

    private void OnActiveAbilityChanged(string activeId)
    {
        foreach (var icon in _icons) icon.Portrait.SetArmed(icon.Ability.Id == activeId);
        RefreshAbilityTooltip();
    }

    private void RefreshAbilityTooltip()
    {
        if (_detached != null) return;
        var player = _gridManager.Player;
        var selectedId = (player.State as AbilitySelectedState)?.AbilityId;
        var active = selectedId == null ? null : _icons.Find(icon => icon.Ability.Id == selectedId);
        if (active != null)
        {
            if (!IsShowing(active)) ShowTooltip(active);
        }
        else if (!ReferenceEquals(_tooltipIcon, null) && _tooltipIcon.Ability != null) HideTooltip();
    }

    private void OnIconClicked(Icon icon)
    {
        if (icon.Ability != null && _detached == null) _gridManager.Player.OnAbilityActivate(icon.Ability, icon.Source);
        else if (IsShowing(icon)) HideTooltip();
        else ShowTooltip(icon);
    }

    private void ShowTooltip(Icon icon)
    {
        var state = _gridManager.GridState;
        var content = icon.Ability != null ? TooltipText.ForAbility(icon.Ability, state, icon.Source.Entity)
            : TooltipText.ForStatus(icon.Status, state);
        _tooltipSource = (RectTransform)icon.Portrait.transform;
        _tooltips.OpenRoot(this, content, _tooltipSource, TooltipSide.Right);
        _tooltipIcon = icon;
    }

    private void HideTooltip()
    {
        _tooltips.Close(this);
        _tooltipIcon = null;
        _tooltipSource = null;
    }

    private bool IsShowing(Icon icon) => ReferenceEquals(_tooltipIcon, icon) && _tooltips.IsOpenFor(this, _tooltipSource);

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
        _unit.SetActive(true);
        _unitName.text = entity.Id;
        _portrait.Init(entityAssets.Model2D);
        _portrait.SetCost(IdRegistry<EntityConfig>.TryGet(entity.Id, out var config) ? config.Cost : 0);
        _portrait.SetHealthVisible(entity.TryGetComponent<HealthComponent>(out _));
        _portrait.Health.SetOwner(entity, _gridManager.ViewerSeat);
        _labelUid = entity.Uid;
        
        if (entity.TryGetComponent<AbilityComponent>(out var abilities))
            AddAbilityIcons(abilities, source);
        RenderShown();
    }

    private void AddAbilityIcons(AbilityComponent abilities, GridSource source)
    {
        Log($"Selection info: {source.Entity.Id}, abilities={abilities.List.Count}");
        foreach (var ability in abilities.List)
        {
            var icon = new Icon { Portrait = Instantiate(_abilityIconPrefab, _container), Ability = ability, Source = source };
            icon.Portrait.Configure(_abilityElements);
            icon.Portrait.Init(IdRegistry<AbilityVisuals>.TryGet(ability.Id, out var visuals) ? visuals.Icon : null);
            icon.Portrait.SetMana(ability.IsCastable ? ability.ManaCost : null);
            icon.Portrait.Button.OnClickCompleted += () => OnIconClicked(icon);
            _icons.Add(icon);
        }
        var activeId = _gridManager.Player.ActiveAbilityId;
        foreach (var icon in _icons) icon.Portrait.SetArmed(icon.Ability.Id == activeId);
    }

    private void RenderAbilities(Entity real, Entity shown)
    {
        if (real == null || shown == null) return;
        if (!real.TryGetComponent<AbilityComponent>(out var realAbilities)
            || !shown.TryGetComponent<AbilityComponent>(out var shownAbilities)) return;
        var seat = _gridManager.LocalSeat;
        for (var i = 0; i < _icons.Count; i++)
            RenderAbility(_icons[i].Portrait, shownAbilities.List[i], _detached != null || _gridManager.GridState.CanCastAt(real.Position, realAbilities.List[i], seat));
    }

    private void RenderAbility(SquarePortrait portrait, Ability shown, bool castable)
    {
        var onCooldown = shown.IsCastable && shown.Cooldown > 0;
        portrait.SetCooldown(onCooldown ? shown.Cooldown : null);
        portrait.SetMana(shown.IsCastable ? shown.ManaCost : null);
        portrait.SetManaDimmed(onCooldown);
        portrait.SetImageColor(castable || !shown.IsCastable ? Color.white : _disabledTint);
    }

    private void RenderStatuses(Entity shown)
    {
        UnityUtil.DestroyAllChildren(_statusContainer.gameObject);
        if (shown == null || !shown.TryGetComponent<StatusComponent>(out var statuses)) return;
        foreach (var status in statuses.Statuses)
        {
            if (!IdRegistry<StatusAssets>.TryGet(status.Id, out var statusAssets)) continue;
            var icon = new Icon { Portrait = Instantiate(_statusIconPrefab, _statusContainer), Status = status };
            icon.Portrait.Configure(_statusElements);
            icon.Portrait.Init(statusAssets.Icon);
            icon.Portrait.SetCount(status.Count);
            icon.Portrait.Button.OnClickCompleted += () => OnIconClicked(icon);
        }
    }

    private void RenderHealth(Entity real, Entity shown)
    {
        if (real == null || !real.TryGetComponent<HealthComponent>(out var health)) return;
        var next = shown != null && shown.TryGetComponent<HealthComponent>(out var shownHealth) ? shownHealth.Current : 0;
        _portrait.Health.SetHealthCount(next);
        _portrait.Health.SetPrevious(health.Current);
        _portrait.Health.SetDead(shown == null);
    }

    private void ClearMenu()
    {
        HideTooltip();
        _unit.SetActive(false);
        foreach (var icon in _icons) Destroy(icon.Portrait.gameObject);
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