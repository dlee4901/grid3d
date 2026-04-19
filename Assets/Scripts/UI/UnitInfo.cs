using UnityEngine;
using UnityEngine.UI;

public class UnitInfo : MonoBehaviour
{
    [SerializeField] private VerticalLayoutGroup _container;
    [SerializeField] private GameObject _squareIconPrefab;
    [SerializeField] private GameObject _rectangleIconPrefab;
    
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
    
    public void DisplayEntityInfo(Entity entity)
    {
        if (IdRegistry<EntityAssets>.TryGet(entity.Id, out var entityAssets))
            return;
        if (!entity.TryGetComponent<SkillComponent>(out var skills))
            return;
        EngineUtil.DestroyAllChildren(_container.gameObject);
        var skillIcons = entityAssets.SkillIcons;
        foreach (var skill in skills.List)
        {
            
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