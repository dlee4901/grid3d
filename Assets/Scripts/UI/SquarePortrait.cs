using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Flags]
public enum PortraitElements
{
    None = 0,
    Health = 1,
    Mana = 2,
    Cost = 4,
    Counter = 8,
    Cooldown = 16,
}

public class SquarePortrait : LoggableBehaviour
{
    [SerializeField] private PortraitElements _elements = PortraitElements.Cost;
    [SerializeField] private InteractableUI _button;
    [SerializeField] private RectTransform _visual;
    [SerializeField] private Image _portrait;
    [SerializeField] private Image _border;
    [SerializeField] private Health _health;
    [SerializeField] private Counter _topCounter;
    [SerializeField] private Counter _bottomCounter;
    [SerializeField] private GameObject _cooldown;
    [SerializeField] private TMP_Text _cooldownText;
    [SerializeField] private Color32 _borderColor = new(255, 255, 255, 64);
    [SerializeField] private Color32 _armedBorderColor = new(255, 192, 64, 255);
    [SerializeField] private Color32 _placedTint = new(96, 96, 96, 255);

    public InteractableUI Button => _button;
    public RectTransform Visual => _visual;
    public Health Health => _health;
    public Image Image => _portrait;

    public void Configure(PortraitElements elements) => _elements = elements;

    public void Init(Sprite sprite)
    {
        SetImage(sprite);
        SetImageColor(Color.white);
        SetHealthVisible(false);
        SetMana(null);
        SetCost(null);
        SetCount(null);
        SetCooldown(null);
    }

    public void SetImage(Sprite sprite) => _portrait.sprite = sprite;

    public void SetImageColor(Color color) => _portrait.color = color;

    public void SetHealthVisible(bool visible) => _health.gameObject.SetActive(Shows(PortraitElements.Health) && visible);

    public void SetMana(int? mana) => SetBadge(_topCounter, PortraitElements.Mana, CounterStyle.Mana, mana);

    public void SetManaDimmed(bool dimmed) => _topCounter.SetDimmed(dimmed);

    public void SetCost(int? cost) => SetBadge(_topCounter, PortraitElements.Cost, CounterStyle.Cost, cost);

    public void SetCount(int? count) => SetBadge(_bottomCounter, PortraitElements.Counter, CounterStyle.Default, count);

    public void SetCooldown(int? turns)
    {
        var visible = Shows(PortraitElements.Cooldown) && turns.HasValue;
        _cooldown.SetActive(visible);
        if (visible) _cooldownText.text = turns.Value.ToString();
    }

    public void SetBorderColor(Color color) => _border.color = color;

    public void SetArmed(bool armed) => _border.color = armed ? _armedBorderColor : _borderColor;

    public void SetPlaced(bool placed)
    {
        _portrait.color = placed ? _placedTint : Color.white;
        _topCounter.SetDimmed(placed);
    }

    private bool Shows(PortraitElements element) => (_elements & element) != 0;

    private void SetBadge(Counter badge, PortraitElements element, CounterStyle style, int? value)
    {
        var visible = Shows(element) && value.HasValue;
        badge.gameObject.SetActive(visible);
        if (!visible) return;
        badge.SetStyle(style);
        badge.SetValue(value.Value);
    }
}
