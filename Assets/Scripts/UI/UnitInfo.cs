using UnityEngine;
using UnityEngine.UI;

public class UnitInfo : LoggableBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private VerticalLayoutGroup _container;
    [SerializeField] private SkillIcon _skillIcon;

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
        Log("DisplayEntityInfo1");
        if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var entityAssets))
            return;
        Log("DisplayEntityInfo2");
        if (!entity.TryGetComponent<SkillComponent>(out var skills))
            return;
        Log("DisplayEntityInfo skills.List.Count " + skills.List.Count);
        var skillIcons = entityAssets.SkillIcons;
        for (var i = 0; i < skills.List.Count; i++)
        {
            if (skillIcons.Count <= i)
                break;
            var icon = Instantiate(_skillIcon, _container.transform);
            icon.Init(skillIcons[i], skills.List[i], ctx);
            icon.OnPreviewRequested  += _gridManager.Player.OnSkillPreview;
            icon.OnPreviewCancelled  += _gridManager.Player.OnSkillCancelPreview;
            icon.OnActivateRequested += _gridManager.Player.OnSkillActivate;
        }
        Log("DisplayEntityInfo end");
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