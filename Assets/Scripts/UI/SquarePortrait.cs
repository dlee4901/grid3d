using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[Flags]
public enum PortraitElements
{
    None = 0,
    Health = 1,
    Cost = 4,
    Counter = 8,
    Cooldown = 16,
}

public enum PortraitCorner { TopLeft, TopRight, BottomLeft, BottomRight }

public class SquarePortrait : LoggableBehaviour
{
    [SerializeField] private PortraitElements _elements = PortraitElements.Cost;
    [SerializeField] private InteractableUI _button;
    [SerializeField] private RectTransform _visual;
    [SerializeField] private Image _portrait;
    [SerializeField] private Image _border;
    [SerializeField] private Health _health;
    [FormerlySerializedAs("_topCounter")] [SerializeField] private Counter _counter;
    [SerializeField] private GameObject _cooldown;
    [SerializeField] private TMP_Text _cooldownText;
    [SerializeField] private Color32 _borderColor = new(255, 255, 255, 64);
    [SerializeField] private Color32 _armedBorderColor = new(255, 192, 64, 255);
    [SerializeField] private Color32 _placedTint = new(96, 96, 96, 255);

    public InteractableUI Button => _button;
    public RectTransform Visual => _visual;
    public Health Health => _health;
    public Image Image => _portrait;

    public Rect TooltipAnchor
    {
        get
        {
            var rect = (RectTransform)transform;
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            var counter = (RectTransform)_counter.transform;
            var reachX = Mathf.Max(0f, counter.anchoredPosition.x * Outward(counter.anchorMin.x)) * rect.lossyScale.x;
            return Rect.MinMaxRect(corners[0].x - reachX, corners[0].y, corners[2].x + reachX, corners[2].y);
        }
    }

    public void Configure(PortraitElements elements) => _elements = elements;

    public void SetShadowDirection(DropShadow.Direction direction)
    {
        foreach (var shadow in GetComponentsInChildren<DropShadow>(true)) shadow.SetDirection(direction);
    }

    public void PlaceCounter(PortraitCorner corner) => Place((RectTransform)_counter.transform, corner);

    public void PlaceHealth(PortraitCorner corner) => Place((RectTransform)_health.transform, corner);

    public void Init(Sprite sprite)
    {
        SetImage(sprite);
        SetImageColor(Color.white);
        SetHealthVisible(false);
        SetCost(null);
        SetCount(null);
        SetCooldown(null);
    }

    public void SetImage(Sprite sprite) => _portrait.sprite = sprite;

    public void SetImageColor(Color color) => _portrait.color = color;

    public void SetHealthVisible(bool visible) => _health.gameObject.SetActive(Shows(PortraitElements.Health) && visible);

    public void SetCost(int? cost) => SetBadge(_counter, PortraitElements.Cost, CounterStyle.Cost, cost);

    public void SetCount(int? count) => SetBadge(_counter, PortraitElements.Counter, CounterStyle.Default, count);

    public void SetCooldownCounter(int? turns) => SetBadge(_counter, PortraitElements.Counter, CounterStyle.Clock, turns);

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
        _counter.SetDimmed(placed);
    }

    private bool Shows(PortraitElements element) => (_elements & element) != 0;

    private static void Place(RectTransform rect, PortraitCorner corner)
    {
        var from = rect.anchorMin;
        var outward = new Vector2(rect.anchoredPosition.x * Outward(from.x), rect.anchoredPosition.y * Outward(from.y));
        var to = new Vector2(corner is PortraitCorner.TopRight or PortraitCorner.BottomRight ? 1f : 0f,
                             corner is PortraitCorner.TopLeft or PortraitCorner.TopRight ? 1f : 0f);
        rect.anchorMin = rect.anchorMax = rect.pivot = to;
        rect.anchoredPosition = new Vector2(outward.x * Outward(to.x), outward.y * Outward(to.y));
    }

    private static float Outward(float anchor) => anchor > 0.5f ? 1f : -1f;

    private void SetBadge(Counter badge, PortraitElements element, CounterStyle style, int? value)
    {
        var visible = Shows(element) && value.HasValue;
        badge.gameObject.SetActive(visible);
        if (!visible) return;
        badge.SetStyle(style);
        badge.SetValue(value.Value);
    }
}
