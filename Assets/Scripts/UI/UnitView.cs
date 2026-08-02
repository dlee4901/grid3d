using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitView : StateView
{
    [SerializeField] private VerticalLayoutGroup _container;
    [SerializeField] private AbilityIcon _abilityIcon;

    private readonly List<AbilityIcon> _icons = new();

    protected override void Start()
    {
        base.Start();
        _gridManager.Player.SelectionChanged += OnInputChanged;
        _gridManager.Player.OnActiveAbilityChanged += OnActiveAbilityChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_gridManager == null) return;
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

    private void OnInputChanged(QueryContext? ctx)
    {
        ClearMenu();
        if (ctx.HasValue) DisplayEntityInfo(ctx.Value);
    }
    
    // public void DisplayPositionInfo(Grid2D grid, int position)
    // {
    //     if (!grid.IsValidPosition(position)) 
    //         return;
    //     var entity = grid.GetEntity(position);
    //     if (entity != null)
    //         DisplayEntityInfo(entity);
    // }
    
    private void DisplayEntityInfo(QueryContext ctx)
    {
        var entity = ctx.SourceEntity;
        if (entity == null)
            return;
        if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var entityAssets))
            return;
        if (!entity.TryGetComponent<AbilityComponent>(out var abilities))
            return;
        Log($"Selection info: {entity.Id}, abilities={abilities.List.Count}");
        var actionable = ctx.Grid.IsAvailableControllable(ctx.SourcePosition);
        var abilityIcons = entityAssets.AbilityIcons;
        for (var i = 0; i < abilities.List.Count; i++)
        {
            if (abilityIcons.Count <= i)
                break;
            var icon = Instantiate(_abilityIcon, _container.transform);
            icon.Init(abilityIcons[i], abilities.List[i], ctx);
            icon.OnPreviewRequested  += _gridManager.Player.OnAbilityPreview;
            icon.OnPreviewCancelled  += _gridManager.Player.OnAbilityCancelPreview;
            icon.OnActivateRequested += _gridManager.Player.OnAbilityActivate;
            icon.SetInteractable(actionable);
            _icons.Add(icon);
        }
        var activeId = _gridManager.Player.ActiveAbilityId;
        foreach (var icon in _icons) icon.SetActiveVisual(icon.AbilityId == activeId);
    }

    private void ClearMenu()
    {
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