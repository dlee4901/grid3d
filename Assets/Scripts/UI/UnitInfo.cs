using UnityEngine;
using UnityEngine.UI;

public class UnitInfo : LoggableBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private VerticalLayoutGroup _container;
    [SerializeField] private AbilityIcon _abilityIcon;

    private void Start()
    {
        _gridManager.Input.OnSelectionChanged += OnInputChanged;
    }

    private void OnDestroy()
    {
        if (_gridManager != null && _gridManager.Input != null)
            _gridManager.Input.OnSelectionChanged -= OnInputChanged;
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
        }
    }
    
    private void ClearMenu()
    {
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