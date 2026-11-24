using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResolutionOrderView : StateView
{
    [SerializeField] private HorizontalLayoutGroup _container;
    [SerializeField] private SquarePortrait _iconPrefab;
    [SerializeField] private PortraitElements _iconElements = PortraitElements.Health;
    [SerializeField] private float _slideSmoothTime = 0.15f;
    [SerializeField] private int _dragSortingOrder = 10;

    [SerializeField] private Color _selectedColor = new Color32(255, 255, 255, 160);
    [SerializeField] private Color _unhighlightedColor = new Color32(255, 255, 255, 64);
    [SerializeField] private HighlightPalette _palette;
    [SerializeField] private bool _reorderable;

    public event Action<IReadOnlyList<int>> Reordered;
        
    private sealed class Icon
    {
        public SquarePortrait Portrait;
        public Entity Entity;
        public float SlideVelocity;
        public bool Dragging;
        public Canvas DragCanvas;
        public RectTransform Rect => (RectTransform)Portrait.transform;
    }

    private readonly List<Icon> _icons = new();
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
            var icon = new Icon { Portrait = Instantiate(_iconPrefab, _container.transform) };
            icon.Portrait.Configure(_iconElements);
            icon.Portrait.Button.OnClickCompleted += () => OnIconSelected(icon.Entity);
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
            _icons[i].Portrait.gameObject.SetActive(active);
            if (!active) continue;
            var uid = entities[i].Uid;
            _shownUids.Add(uid);
            Bind(_icons[i], entities[i]);
            if (i < previous.Count && previous[i] == uid) continue;
            var from = snap ? -1 : baseline.IndexOf(uid);
            var offset = from < 0 || from == i ? 0f : (from - i) * Stride(_icons[i]);
            SlideFrom(_icons[i], offset);
        }
        _shownPreview = previewing;
        RefreshOutlines();
    }

    private void ForgetShown() => _shownUids.Clear();

    private void Bind(Icon icon, Entity entity)
    {
        icon.Entity = entity;
        var portrait = icon.Portrait;
        portrait.Init(IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets) ? assets.Model2D : null);
        var hasHealth = entity.TryGetComponent<HealthComponent>(out var health);
        portrait.SetHealthVisible(hasHealth);
        if (!hasHealth) return;
        portrait.Health.SetHealthCount(health.Current);
        portrait.Health.SetOwner(entity, _gridManager.ViewerSeat);
    }

    private void SlideFrom(Icon icon, float offset)
    {
        icon.Portrait.Visual.anchoredPosition = new Vector2(offset, 0f);
        icon.SlideVelocity = 0f;
    }

    private void AddDragRelay(Icon icon)
    {
        var relay = icon.Portrait.gameObject.AddComponent<DragRelay>();
        relay.Began += _ => BeginDrag(icon);
        relay.Moved += data => DragIcon(icon, data);
        relay.Ended += _ => EndDrag(icon);
    }

    private void BeginDrag(Icon icon)
    {
        icon.Dragging = true;
        icon.DragCanvas = icon.Portrait.gameObject.AddComponent<Canvas>();
        icon.DragCanvas.overrideSorting = true;
        icon.DragCanvas.sortingOrder = _dragSortingOrder;
    }

    private void DragIcon(Icon icon, PointerEventData data)
    {
        var container = (RectTransform)_container.transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(container, data.position, data.pressEventCamera, out var local)) return;
        var rect = icon.Rect;
        var stride = Stride(icon);
        var from = _icons.IndexOf(icon);
        var to = Mathf.Clamp(Mathf.RoundToInt((local.x - _container.padding.left - rect.rect.width / 2f) / stride), 0, _shownUids.Count - 1);
        if (to != from)
        {
            var step = to > from ? 1 : -1;
            for (var i = from + step; i != to + step; i += step) SlideFrom(_icons[i], step * stride);
            _icons.RemoveAt(from);
            _icons.Insert(to, icon);
            var uid = _shownUids[from];
            _shownUids.RemoveAt(from);
            _shownUids.Insert(to, uid);
            rect.SetSiblingIndex(to);
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        }
        icon.Portrait.Visual.anchoredPosition = new Vector2(local.x - rect.anchoredPosition.x, 0f);
    }

    private void EndDrag(Icon icon)
    {
        icon.Dragging = false;
        Destroy(icon.DragCanvas);
        SlideFrom(icon, icon.Portrait.Visual.anchoredPosition.x);
        Reordered?.Invoke(new List<int>(_shownUids));
    }

    private float Stride(Icon icon) => icon.Rect.rect.width + _container.spacing;

    private void LateUpdate()
    {
        foreach (var icon in _icons) Slide(icon);
        if (!_outlinesDirty) return;
        _outlinesDirty = false;
        RefreshOutlines();
    }

    private const float SlideSettlePixels = 0.5f;

    private void Slide(Icon icon)
    {
        if (icon.Dragging) return;
        var visual = icon.Portrait.Visual;
        var x = visual.anchoredPosition.x;
        if (x == 0f) return;
        var next = Mathf.SmoothDamp(x, 0f, ref icon.SlideVelocity, _slideSmoothTime);
        if (Mathf.Abs(next) < SlideSettlePixels)
        {
            next = 0f;
            icon.SlideVelocity = 0f;
        }
        visual.anchoredPosition = new Vector2(next, 0f);
    }

    private void MarkOutlinesDirty() => _outlinesDirty = true;

    private void OnSelectionChanged(GridSource? selection) => _outlinesDirty = true;


    private void RefreshOutlines()
    {
        var selected = _gridManager.Player.CurrentSelection?.Entity;

        foreach (var icon in _icons)
        {
            if (!icon.Portrait.gameObject.activeSelf || icon.Entity == null) continue;
            icon.Portrait.SetBorderColor(BorderColor(icon.Entity, selected));
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
        => type is HighlightType.AvailableSources or HighlightType.EffectArea or HighlightType.ConfirmTarget;

    private void OnIconSelected(Entity entity)
    {
        var real = entity == null ? null : _gridManager.GridState.GetEntityByUid(entity.Uid);
        if (real == null) return;
        _gridManager.Player.OnPositionSelected(new GridSource(_gridManager.GridState, real.Position, real));
    }
}