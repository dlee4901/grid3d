using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthCounter : LoggableBehaviour
{
    [SerializeField] private Image _healthIcon;
    [SerializeField] private TMP_Text _counterText;
    
    [SerializeField] private float _size = 48f;
    [SerializeField] private Color _lossColor = new Color32(255, 80, 80, 255);
    [SerializeField] private Color _gainColor = new Color32(80, 220, 120, 255);

    private int _current = int.MinValue;
    private Transform _counterTextTransform;
    private Color? _textColor;
    
    public bool ForceVisible { get; set; }
    public int Change { get; private set; }

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
    
    public void SetChange(int change)
    {
        if (change == Change) return;
        _textColor ??= _counterText.color;
        Change = change;
        _counterText.color = change < 0 ? _lossColor : change > 0 ? _gainColor : _textColor.Value;
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