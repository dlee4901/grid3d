using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionOrderView : StateView
{
    [SerializeField] private HorizontalLayoutGroup _container;
    [SerializeField] private ResolutionOrderIcon _iconPrefab;

    [SerializeField] private Color _selectedColor = new Color32(255, 255, 255, 160);
    [SerializeField] private Color _unhighlightedColor = new Color32(255, 255, 255, 0);
        
    private readonly List<ResolutionOrderIcon> _icons = new();
    private bool _outlinesDirty;

    protected override void OnGameStarted()
    {
        _gridManager.Player.SelectionChanged += OnSelectionChanged;
        _gridManager.Player.Highlights.Changed += MarkOutlinesDirty;
        base.OnGameStarted();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_gridManager == null || _gridManager.Player == null) return;
        _gridManager.Player.SelectionChanged -= OnSelectionChanged;
        if (_gridManager.Player.Highlights != null) _gridManager.Player.Highlights.Changed -= MarkOutlinesDirty;
    }
    
    protected override void Refresh()
    {
        var state = _gridManager.GridState;
        var entities = state.ResolutionOrder.Entities;

        while (_icons.Count < entities.Count)
        {
            var icon = Instantiate(_iconPrefab, _container.transform);
            icon.OnSelectRequested += OnIconSelected;
            _icons.Add(icon);
        }

        for (var i = 0; i < _icons.Count; i++)
        {
            var active = i < entities.Count;
            _icons[i].gameObject.SetActive(active);
            if (active) _icons[i].Bind(entities[i], state.ActivePlayer);
        }
        RefreshOutlines();
    }

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
        var player = _gridManager.Player;
        var selected = player.CurrentSelection?.Entity;

        foreach (var icon in _icons)
        {
            if (!icon.gameObject.activeSelf || icon.Entity == null) continue;
            icon.SetOutlineColor(OutlineColor(icon.Entity, selected, player));
        }
    }
  
    private Color OutlineColor(IReadOnlyEntity entity, IReadOnlyEntity selected, PlayerInputController player)
    {
        if (ReferenceEquals(entity, selected)) return _selectedColor;
        if (player.Highlights.TryGet(entity.Position, out var type) && SyncsToStrip(type)) 
            return player.Highlights.HighlightColor(type);
        return _unhighlightedColor;
    }

    private static bool SyncsToStrip(GridHighlightType type) 
        => type is GridHighlightType.AvailableEntities or GridHighlightType.EffectPreview;

    private void OnIconSelected(IReadOnlyEntity entity)
    {
        if (entity == null) return;
        _gridManager.Player.OnPositionSelected(new GridSource(_gridManager.GridState, entity.Position, entity));
    }
}