using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthCounter : LoggableBehaviour
{
    [SerializeField] private Image _healthIcon;
    [SerializeField] private TMP_Text _counterText;
    
    [SerializeField] private float _size = 32f;

    private int _current = int.MinValue;

    private void Start()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(_size, _size);
        _counterText.fontSize = _size / 2f;
    }

    public void SetHealthCount(int health)
    {
        if (health == _current) return;
        _current = health;
        _counterText.text = health.ToString();
    }
}