using TMPro;
using UnityEngine;

public class UnitLabel : LoggableBehaviour
{
    [SerializeField] private TextMeshProUGUI _unitName;
    [SerializeField] private UnitPortrait _portrait;
    
    public HealthCounter HealthCounter => _portrait.Health;
    
    public void Init(string unitName, Sprite portrait, int cost)
    {
        _unitName.text = unitName;
        _portrait.Init(portrait, cost);
    }
}
