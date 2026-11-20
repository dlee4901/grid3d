using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TooltipStack : MonoBehaviour
{
    [SerializeField] private Tooltip _tooltipPrefab;

    private sealed class Chain
    {
        public Object Owner;
        public RectTransform Source;
        public readonly List<Tooltip> Open = new();
    }

    private readonly List<Chain> _chains = new();

    public void OpenRoot(Object owner, TooltipContent content, RectTransform source, TooltipSide side)
    {
        var chain = ChainOf(owner);
        if (chain == null) _chains.Add(chain = new Chain { Owner = owner });
        CloseFrom(chain, 0);
        chain.Source = source;
        Open(chain, content, VisibleRect(source), side);
    }

    public bool IsOpenFor(Object owner, RectTransform source)
        => ChainOf(owner) is { } chain && chain.Open.Count > 0 && ReferenceEquals(chain.Source, source);

    public void Close(Object owner)
    {
        if (ChainOf(owner) is { } chain) CloseFrom(chain, 0);
    }

    private Chain ChainOf(Object owner) => _chains.Find(chain => chain.Owner == owner);

    private void Open(Chain chain, TooltipContent content, Rect anchor, TooltipSide side)
    {
        var tooltip = Instantiate(_tooltipPrefab, transform);
        tooltip.LinkPressed += (parent, link, bounds) => OnLinkPressed(chain, parent, link, bounds);
        chain.Open.Add(tooltip);
        tooltip.Show(content, anchor, side);
    }

    private void OnLinkPressed(Chain chain, Tooltip parent, string link, Rect anchor)
    {
        if (!parent.Content.Links.TryGetValue(link, out var content)) return;
        CloseFrom(chain, chain.Open.IndexOf(parent) + 1);
        Open(chain, content, anchor, parent.Side);
    }

    private void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;
        var position = pointer.position.ReadValue();
        var pressed = FindPressed(position, out var deepest);
        foreach (var chain in _chains)
            if (chain != pressed) CloseFrom(chain, 1);
        if (pressed == null) return;
        CloseFrom(pressed, deepest + 1);
        if (pressed.Open[deepest].TryGetLink(position, out var id, out var bounds))
            OnLinkPressed(pressed, pressed.Open[deepest], id, bounds);
    }

    private Chain FindPressed(Vector2 position, out int deepest)
    {
        for (var c = _chains.Count - 1; c >= 0; c--)
        {
            var open = _chains[c].Open;
            for (deepest = open.Count - 1; deepest >= 0; deepest--)
                if (open[deepest].Contains(position)) return _chains[c];
        }
        deepest = -1;
        return null;
    }

    private static void CloseFrom(Chain chain, int index)
    {
        for (var i = chain.Open.Count - 1; i >= index; i--)
        {
            Destroy(chain.Open[i].gameObject);
            chain.Open.RemoveAt(i);
        }
        if (chain.Open.Count == 0) chain.Source = null;
    }

    private static Rect VisibleRect(RectTransform source)
    {
        var corners = new Vector3[4];
        var min = new Vector2(float.MaxValue, float.MaxValue);
        var max = new Vector2(float.MinValue, float.MinValue);
        foreach (var graphic in source.GetComponentsInChildren<Graphic>())
        {
            if (!graphic.enabled || graphic.color.a <= 0f) continue;
            graphic.rectTransform.GetWorldCorners(corners);
            min = Vector2.Min(min, corners[0]);
            max = Vector2.Max(max, corners[2]);
        }
        if (min.x > max.x)
        {
            source.GetWorldCorners(corners);
            min = corners[0];
            max = corners[2];
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }
}
