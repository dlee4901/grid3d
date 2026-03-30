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
    
    public void DisplayEntityInfo(Entity entity)
    {
        
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