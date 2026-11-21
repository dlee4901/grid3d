using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResolutionOrderView : StateView
{
    [SerializeField] private HorizontalLayoutGroup _container;
    [SerializeField] private ResolutionOrderIcon _iconPrefab;

    [SerializeField] private Color _selectedColor = new Color32(255, 255, 255, 160);
    [SerializeField] private Color _unhighlightedColor = new Color32(255, 255, 255, 64);
    [SerializeField] private HighlightPalette _palette;
    [SerializeField] private bool _reorderable;

    public event Action<IReadOnlyList<int>> Reordered;
        
    private readonly List<ResolutionOrderIcon> _icons = new();
    private readonly List<int> _shownUids = new();
    private bool _shownPreview;
    private bool _outlinesDirty;

    protected override void OnGameStarted()
    {
        _gridManager.Player.SelectionChanged += OnSelectionChanged;
        _gridManager.Renderer.Highlights.HighlightsChanged += MarkOutlinesDirty;
        _gridManager.Restarted += ForgetShown;
        base.OnGameStarted();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_gridManager == null) return;
        _gridManager.Restarted -= ForgetShown;
        if (_gridManager.Player == null) return;
        _gridManager.Player.SelectionChanged -= OnSelectionChanged;
        if (_gridManager.Renderer != null) _gridManager.Renderer.Highlights.HighlightsChanged -= MarkOutlinesDirty;
    }
    
    protected override void Render(GridState shown)
    {
        var entities = shown.ResolutionOrder.Entities;

        while (_icons.Count < entities.Count)
        {
            var icon = Instantiate(_iconPrefab, _container.transform);
            icon.OnSelectRequested += OnIconSelected;
            if (_reorderable) AddDragRelay(icon);
            _icons.Add(icon);
        }

        var previewing = _gridManager.IsPreviewing;
        var previous = new List<int>(_shownUids);
        var baseline = previewing ? _gridManager.GridState.ResolutionOrder.Entities.Select(entity => entity.Uid).ToList() : previous;
        var snap = !previewing && _shownPreview;
        _shownUids.Clear();
        for (var i = 0; i < _icons.Count; i++)
        {
            var active = i < entities.Count;
            _icons[i].gameObject.SetActive(active);
            if (!active) continue;
            var uid = entities[i].Uid;
            _shownUids.Add(uid);
            _icons[i].Bind(entities[i], _gridManager.ViewerSeat);
            if (i < previous.Count && previous[i] == uid) continue;
            var from = snap ? -1 : baseline.IndexOf(uid);
            var offset = from < 0 || from == i ? 0f : (from - i) * Stride(_icons[i]);
            _icons[i].SlideFrom(offset);
        }
        _shownPreview = previewing;
        RefreshOutlines();
    }

    private void ForgetShown() => _shownUids.Clear();

    private void AddDragRelay(ResolutionOrderIcon icon)
    {
        var relay = icon.gameObject.AddComponent<DragRelay>();
        relay.Began += _ => icon.BeginDrag();
        relay.Moved += data => DragIcon(icon, data);
        relay.Ended += _ => EndDrag(icon);
    }

    private void DragIcon(ResolutionOrderIcon icon, PointerEventData data)
    {
        var container = (RectTransform)_container.transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(container, data.position, data.pressEventCamera, out var local)) return;
        var rect = (RectTransform)icon.transform;
        var stride = Stride(icon);
        var from = _icons.IndexOf(icon);
        var to = Mathf.Clamp(Mathf.RoundToInt((local.x - _container.padding.left - rect.rect.width / 2f) / stride), 0, _shownUids.Count - 1);
        if (to != from)
        {
            var step = to > from ? 1 : -1;
            for (var i = from + step; i != to + step; i += step) _icons[i].SlideFrom(step * stride);
            _icons.RemoveAt(from);
            _icons.Insert(to, icon);
            var uid = _shownUids[from];
            _shownUids.RemoveAt(from);
            _shownUids.Insert(to, uid);
            rect.SetSiblingIndex(to);
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        }
        icon.DragTo(local.x - rect.anchoredPosition.x);
    }

    private void EndDrag(ResolutionOrderIcon icon)
    {
        icon.EndDrag();
        Reordered?.Invoke(new List<int>(_shownUids));
    }

    private float Stride(ResolutionOrderIcon icon) => ((RectTransform)icon.transform).rect.width + _container.spacing;

    private void LateUpdate()
    {
        if (!_outlinesDirty) return;
        _outlinesDirty = false;
        RefreshOutlines();
    }

    private void MarkOutlinesDirty() => _outlinesDirty = true;

    private void OnSelectionChanged(GridSource? selection) => _outlinesDirty = true;


    private void RefreshOutlines()
    {
        var selected = _gridManager.Player.CurrentSelection?.Entity;

        foreach (var icon in _icons)
        {
            if (!icon.gameObject.activeSelf || icon.Entity == null) continue;
            icon.SetBorderColor(BorderColor(icon.Entity, selected));
        }
    }
  
    private Color BorderColor(Entity entity, Entity selected)
    {
        if (selected != null && entity.Uid == selected.Uid) return _selectedColor;
        if (_gridManager.Renderer.Highlights.TryGetHighlight(entity.Position, out var type) && SyncsToStrip(type))
            return _palette.UIColorFor(type);
        return _unhighlightedColor;
    }

    private static bool SyncsToStrip(HighlightType type) 
        => type is HighlightType.AvailableSources or HighlightType.EffectArea;

    private void OnIconSelected(Entity entity)
    {
        var real = entity == null ? null : _gridManager.GridState.GetEntityByUid(entity.Uid);
        if (real == null) return;
        _gridManager.Player.OnPositionSelected(new GridSource(_gridManager.GridState, real.Position, real));
    }
}