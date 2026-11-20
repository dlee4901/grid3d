using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TooltipStack : MonoBehaviour
{
    [SerializeField] private Tooltip _tooltipPrefab;

    private readonly List<Tooltip> _open = new();
    private Object _owner;
    private RectTransform _rootSource;

    public void OpenRoot(Object owner, TooltipContent content, RectTransform source, TooltipSide side)
    {
        CloseFrom(0);
        _owner = owner;
        _rootSource = source;
        Open(content, VisibleRect(source), side);
    }

    public bool IsOpenFor(Object owner, RectTransform source)
        => _open.Count > 0 && _owner == owner && ReferenceEquals(_rootSource, source);

    public void Close(Object owner)
    {
        if (_open.Count > 0 && _owner == owner) CloseFrom(0);
    }

    private void Open(TooltipContent content, Rect anchor, TooltipSide side)
    {
        var tooltip = Instantiate(_tooltipPrefab, transform);
        tooltip.LinkPressed += OnLinkPressed;
        _open.Add(tooltip);
        tooltip.Show(content, anchor, side);
    }

    private void OnLinkPressed(Tooltip parent, string link, Rect anchor)
    {
        if (!parent.Content.Links.TryGetValue(link, out var content)) return;
        CloseFrom(_open.IndexOf(parent) + 1);
        Open(content, anchor, parent.Side);
    }

    private void Update()
    {
        if (_open.Count == 0) return;
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;
        var position = pointer.position.ReadValue();
        var deepest = -1;
        for (var i = _open.Count - 1; i >= 0; i--)
        {
            if (!_open[i].Contains(position)) continue;
            deepest = i;
            break;
        }
        CloseFrom(Mathf.Max(1, deepest + 1));
        if (deepest >= 0 && _open[deepest].TryGetLink(position, out var id, out var bounds))
            OnLinkPressed(_open[deepest], id, bounds);
    }

    private void CloseFrom(int index)
    {
        for (var i = _open.Count - 1; i >= index; i--)
        {
            Destroy(_open[i].gameObject);
            _open.RemoveAt(i);
        }
        if (_open.Count > 0) return;
        _owner = null;
        _rootSource = null;
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
