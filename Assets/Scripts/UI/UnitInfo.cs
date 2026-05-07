using UnityEngine;
using UnityEngine.UI;

public class UnitInfo : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup _container;
    [SerializeField] private SkillIcon _skillIcon;
    
    private void Start()
    {
        SelectionController.Singleton.OnSelectionChanged += OnSelectionChanged;
    }

    private void OnDestroy()
    {
        if (SelectionController.Singleton != null)
            SelectionController.Singleton.OnSelectionChanged -= OnSelectionChanged;
    }

    private void OnSelectionChanged(QueryContext? ctx)
    {
        if (!ctx.HasValue || ctx.Value.SourceEntity == null)
        {
            UnityUtil.DestroyAllChildren(_container.gameObject);
            return;
        }
        DisplayEntityInfo(ctx.Value);
    }
    
    // public void DisplayPositionInfo(Grid2D grid, int position)
    // {
    //     if (!grid.IsValidPosition(position)) 
    //         return;
    //     var entity = grid.GetEntity(position);
    //     if (entity != null)
    //         DisplayEntityInfo(entity);
    // }
    
    public void DisplayEntityInfo(QueryContext ctx)
    {
        var entity = ctx.SourceEntity;
        if (entity == null) 
            return;
        if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var entityAssets))
            return;
        if (!entity.TryGetComponent<SkillComponent>(out var skills))
            return;
        UnityUtil.DestroyAllChildren(_container.gameObject);
        var skillIcons = entityAssets.SkillIcons;
        //foreach (var skill in skills.List)
        for (var i = 0; i < skills.List.Count; i++)
        {
            var icon = Instantiate(_skillIcon, _container.transform);
            if (!icon.TryGetComponent<Image>(out var image))
                return;
            var sprite = skillIcons.Count > i ? skillIcons[i] : null;
            icon.Init(sprite, skills.List[i], ctx);
        }
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