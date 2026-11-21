using UnityEngine;
using UnityEngine.UI;

public class UnitPortrait : LoggableBehaviour
{
    [SerializeField] private InteractableUI _button;
    [SerializeField] private Image _portrait;
    [SerializeField] private ManaCounter _cost;
    [SerializeField] private HealthCounter _health;
    [SerializeField] private Image _border;
    [SerializeField] private bool _showCost = true;
    [SerializeField] private bool _showHealth = true;
    [SerializeField] private float _referenceSize = 128f;
    [SerializeField] private float _healthSize = 48f;
    [SerializeField] private float _costSize = 32f;
    [SerializeField] private float _badgeInset = 12f;
    [SerializeField] private Color32 _borderColor = new(255, 255, 255, 64);
    [SerializeField] private Color32 _armedBorderColor = new(255, 192, 64, 255);
    [SerializeField] private Color32 _placedTint = new(96, 96, 96, 255);

    public InteractableUI Button => _button;
    public HealthCounter Health => _health;

    public void Init(Sprite portrait, int cost, bool hasHealth = true)
    {
        _portrait.sprite = portrait;
        _cost.gameObject.SetActive(_showCost);
        if (_showCost) _cost.SetManaCount(cost);
        _health.gameObject.SetActive(_showHealth && hasHealth);
        ApplyScale();
    }

    private void OnRectTransformDimensionsChange() => ApplyScale();

    private void ApplyScale()
    {
        var width = ((RectTransform)transform).rect.width;
        if (width <= 0f) return;
        var scale = width / _referenceSize;
        var inset = _badgeInset * scale;
        _health.SetSize(_healthSize * scale);
        ((RectTransform)_health.transform).anchoredPosition = new Vector2(inset, inset);
        _cost.SetSize(_costSize * scale);
        ((RectTransform)_cost.transform).anchoredPosition = new Vector2(-inset, -inset);
    }

    public void SetBorderColor(Color color) => _border.color = color;

    public void SetArmed(bool armed) => _border.color = armed ? _armedBorderColor : _borderColor;

    public void SetPlaced(bool placed)
    {
        _portrait.color = placed ? _placedTint : Color.white;
        _cost.SetDimmed(placed);
    }
}
