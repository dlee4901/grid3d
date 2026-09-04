using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthCounter : LoggableBehaviour
{
    [SerializeField] private Image _healthIcon;
    [SerializeField] private TMP_Text _counterText;
    
    [SerializeField] private float _size = 48f;

    private int _current = int.MinValue;
    private Transform _counterTextTransform;
    
    public bool ForceVisible { get; set; }

    private void Start()
    {
        _healthIcon.raycastTarget = false;
        _counterText.raycastTarget = false;
        SetSize(_size);
    }
    
    // 18 48
    // 8 24
    
    public void SetSize(float size)
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
        _counterText.fontSize = size / 2f;
    }

    public void SetHealthCount(int health)
    {
        if (health == _current) return;
        _current = health;
        var healthText = health.ToString();
        _counterText.rectTransform.offsetMin = new Vector2(_counterText.rectTransform.offsetMin.x, healthText.Length > 1 ? _size/6f : _size/12f);
        _counterText.text = health.ToString();
    }
    
    public void SetColor(Color color)
    {
        color.a = _healthIcon.color.a;
        _healthIcon.color = color;
    }

    public void SetAlpha(float alpha)
    {
        var color = _healthIcon.color;
        color.a = alpha;
        _healthIcon.color = color;
    }
}