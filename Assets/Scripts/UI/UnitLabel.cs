using TMPro;
using UnityEngine;

public class UnitLabel : LoggableBehaviour
{
    [SerializeField] private HealthCounter _healthCounter;
    [SerializeField] private TextMeshProUGUI _unitName;
    
    public HealthCounter HealthCounter => _healthCounter;
    
    public void SetName(string unitName) => _unitName.text = unitName;
}