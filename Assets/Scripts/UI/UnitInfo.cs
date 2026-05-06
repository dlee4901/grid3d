using UnityEngine;
using UnityEngine.UI;

public class UnitInfo : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup _container;
    [SerializeField] private SkillIcon _skillIcon;
    
    private void Start()
    {
        //EventManager.Singleton.OnSelectUnit += EventManager_OnSelectUnit;
    }
    
    // public void DisplayPositionInfo(Grid2D grid, int position)
    // {
    //     if (!grid.IsValidPosition(position)) 
    //         return;
    //     var entity = grid.GetEntity(position);
    //     if (entity != null)
    //         DisplayEntityInfo(entity);
    // }
    
    public void DisplayEntityInfo(IReadOnlyGridState grid, Entity entity)
    {
        if (IdRegistry<EntityAssets>.TryGet(entity.Id, out var entityAssets))
            return;
        if (!entity.TryGetComponent<SkillComponent>(out var skills))
            return;
        UnityUtil.DestroyAllChildren(_container.gameObject);
        var skillIcons = entityAssets.SkillIcons;
        foreach (var skill in skills.List)
        {
            var icon = Instantiate(_skillIcon, _container.transform);
            if (!icon.TryGetComponent<Image>(out var image))
                return;
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