using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TooltipSide { Right, Left, Below }

public class Tooltip : MonoBehaviour
{
    [SerializeField] private TMP_Text _title;
    [SerializeField] private RectTransform _header;
    [SerializeField] private TMP_Text _body;
    [SerializeField] private Counter _prefab;
    [SerializeField] private CounterStyle _cooldownStyle = CounterStyle.Clock;
    [SerializeField] private ShapeGlyph _shapeGlyphPrefab;
    [SerializeField] private float _counterSize = 48f;
    [SerializeField] private float _gap;
    [SerializeField] private float _frameInset = 2f;
    [SerializeField] private float _minWidth = 384f;

    public RectTransform Panel => (RectTransform)transform;
    public TooltipSide Side { get; private set; }
    public TooltipContent Content { get; private set; }

    public event Action<Tooltip, string, Rect> LinkPressed;

    public void Show(TooltipContent content, Rect anchor, TooltipSide side)
    {
        Content = content;
        _title.text = content.Title;
        var icons = 0;
        if (content.Glyph != null) { AddGlyph(content.Glyph); icons++; }
        if (content.Cooldown.HasValue) { AddCounter(_cooldownStyle, content.Cooldown.Value); icons++; }
        _body.text = content.Body;
        FitWidth(icons);
        Side = side;
        LayoutRebuilder.ForceRebuildLayoutImmediate(Panel);
        Place(anchor, side);
    }

    public bool Contains(Vector2 screenPoint) => RectTransformUtility.RectangleContainsScreenPoint(Panel, screenPoint, null);

    public bool TryGetLink(Vector2 screenPoint, out string id, out Rect bounds)
        => TryGetLink(_body, screenPoint, out id, out bounds);

    private void FitWidth(int icons)
    {
        var spacing = _header.GetComponent<HorizontalLayoutGroup>().spacing;
        var padding = Panel.GetComponent<VerticalLayoutGroup>().padding.horizontal;
        var title = _title.GetPreferredValues(_title.text, float.PositiveInfinity, float.PositiveInfinity).x;
        var header = title + icons * (_counterSize + spacing);
        Panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(_minWidth, Mathf.Ceil(header) + padding));
    }

    private void AddGlyph(GlyphShape shape)
    {
        var glyph = Instantiate(_shapeGlyphPrefab, _header);
        glyph.Show(shape);
        var layout = glyph.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = _counterSize;
        layout.preferredHeight = _counterSize;
    }

    private void AddCounter(CounterStyle style, int value)
    {
        var counter = Instantiate(_prefab, _header);
        ((RectTransform)counter.transform).sizeDelta = new Vector2(_counterSize, _counterSize);
        counter.SetStyle(style);
        counter.SetValue(value);
        var layout = counter.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = _counterSize;
        layout.preferredHeight = _counterSize;
    }

    private void Place(Rect anchor, TooltipSide side)
    {
        var scale = GetComponentInParent<Canvas>().rootCanvas.scaleFactor;
        var size = Panel.rect.size * scale;
        var gap = _gap * scale;
        var inset = _frameInset * scale;
        var topLeft = side switch
        {
            TooltipSide.Right => new Vector2(anchor.xMax + gap - inset, anchor.yMax + inset),
            TooltipSide.Left => new Vector2(anchor.xMin - gap - size.x + inset, anchor.yMax + inset),
            _ => new Vector2(anchor.xMin - inset, anchor.yMin - gap + inset)
        };
        if (side != TooltipSide.Below && topLeft.y - size.y < 0f) topLeft.y = anchor.yMin - inset + size.y;
        topLeft.x = Mathf.Clamp(topLeft.x, 0f, Mathf.Max(0f, Screen.width - size.x));
        topLeft.y = Mathf.Clamp(topLeft.y, Mathf.Min(size.y, Screen.height), Screen.height);
        Panel.position = topLeft;
    }

    private static bool TryGetLink(TMP_Text text, Vector2 screenPoint, out string id, out Rect bounds)
    {
        id = null;
        bounds = default;
        if (!text.gameObject.activeInHierarchy) return false;
        var index = TMP_TextUtilities.FindIntersectingLink(text, screenPoint, null);
        if (index < 0) return false;
        var link = text.textInfo.linkInfo[index];
        id = link.GetLinkID();
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        for (var i = link.linkTextfirstCharacterIndex; i < link.linkTextfirstCharacterIndex + link.linkTextLength; i++)
        {
            var character = text.textInfo.characterInfo[i];
            var bottomLeft = (Vector2)text.transform.TransformPoint(character.bottomLeft);
            var topRight = (Vector2)text.transform.TransformPoint(character.topRight);
            min = Vector2.Min(min, bottomLeft);
            max = Vector2.Max(max, topRight);
        }
        bounds = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return true;
    }
}
