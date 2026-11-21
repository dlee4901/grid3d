using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CounterStyle { Default, Mana, Cost, Clock, Ring }

public class Counter : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Image _icon;
    [SerializeField] private Image _shadow;
    [SerializeField] private CounterStyle _style;

    [SerializeField] private Sprite _circleSprite;
    [SerializeField] private Sprite _clockSprite;
    [SerializeField] private Sprite _ringSprite;

    [SerializeField] private Color32 _defaultColor = new(128, 128, 128, 255);
    [SerializeField] private Color32 _manaColor = new(0, 128, 255, 255);
    [SerializeField] private Color32 _costColor = new(192, 144, 0, 255);
    [SerializeField] private Color32 _cooldownColor = new(255, 255, 255, 255);

    [SerializeField] private Color32 _dimmedTint = new(128, 128, 128, 255);

#if UNITY_EDITOR
    private void OnValidate() => ApplyStyle();
#endif

    public void SetStyle(CounterStyle style)
    {
        _style = style;
        ApplyStyle();
    }

    public void SetText(string text)
    {
        _text.text = text;
    }

    public void SetValue(int value)
    {
        _text.text = value.ToString();
    }

    public void SetDimmed(bool dimmed)
    {
        var tint = dimmed ? (Color)_dimmedTint : Color.white;
        foreach (var graphic in GetComponentsInChildren<Graphic>()) graphic.canvasRenderer.SetColor(tint);
    }

    private void ApplyStyle()
    {
        var sprite = _style switch
        {
            CounterStyle.Clock => _clockSprite,
            CounterStyle.Ring => _ringSprite,
            _ => _circleSprite,
        };
        _icon.sprite = sprite;
        _shadow.sprite = sprite;
        _icon.color = _style switch
        {
            CounterStyle.Mana => _manaColor,
            CounterStyle.Cost => _costColor,
            CounterStyle.Clock or CounterStyle.Ring => _cooldownColor,
            _ => _defaultColor,
        };
    }
}
