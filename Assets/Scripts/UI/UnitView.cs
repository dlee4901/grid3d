using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UnitView : StateView
{
    [SerializeField] private VerticalLayoutGroup _container;
    [SerializeField] private UnitLabel _unitLabelPrefab;
    [FormerlySerializedAs("_abilityIcon")] [SerializeField] private AbilityIcon _abilityIconPrefab;
    [SerializeField] private RectTransform _panel;
    [SerializeField] private float _slideDuration = 0.2f;

    private readonly List<AbilityIcon> _icons = new();
    private UnitLabel _unitLabel;
    private int _labelUid;
    private float _shownX;
    private Coroutine _slide;

    private float HiddenX => _shownX - _panel.rect.width;

    protected override bool ShowsPreview => false;

    protected override void Start()
    {
        _shownX = _panel.anchoredPosition.x;
        SetPanelX(HiddenX);
        base.Start();
        _gridManager.Player.SelectionChanged += OnInputChanged;
        _gridManager.Player.OnActiveAbilityChanged += OnActiveAbilityChanged;
        _gridManager.DisplayChanged += RefreshLabelHealth;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (_gridManager == null) return;
        _gridManager.DisplayChanged -= RefreshLabelHealth;
        if (_gridManager.Player != null)
        {
            _gridManager.Player.SelectionChanged -= OnInputChanged;
            _gridManager.Player.OnActiveAbilityChanged -= OnActiveAbilityChanged;
        }
    }

    protected override void Refresh() => OnInputChanged(_gridManager.Player.CurrentSelection);

    private void OnActiveAbilityChanged(string activeId)
    {
        foreach (var icon in _icons) icon.SetActiveVisual(icon.AbilityId == activeId);
    }

    private void OnInputChanged(GridSource? source)
    {
        if (source?.Entity == null)
        {
            SlideTo(HiddenX, ClearMenu);
            return;
        }
        ClearMenu();
        DisplayEntityInfo(source.Value);
        SlideTo(_shownX, null);
    }

    private void SlideTo(float x, Action onArrived)
    {
        if (_slide != null) StopCoroutine(_slide);
        _slide = StartCoroutine(Slide(x, onArrived));
    }

    private IEnumerator Slide(float x, Action onArrived)
    {
        var startX = _panel.anchoredPosition.x;
        var duration = _slideDuration * Mathf.Abs(x - startX) / _panel.rect.width;
        for (var t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            var eased = 1f - Mathf.Pow(1f - t / duration, 3f);
            SetPanelX(Mathf.Lerp(startX, x, eased));
            yield return null;
        }
        SetPanelX(x);
        _slide = null;
        onArrived?.Invoke();
    }

    private void SetPanelX(float x) => _panel.anchoredPosition = new Vector2(x, _panel.anchoredPosition.y);
    
    // public void DisplayPositionInfo(Grid2D grid, int position)
    // {
    //     if (!grid.IsValidPosition(position)) 
    //         return;
    //     var entity = grid.GetEntity(position);
    //     if (entity != null)
    //         DisplayEntityInfo(entity);
    // }
    
    private void DisplayEntityInfo(GridSource source)
    {
        var entity = source.Entity;
        if (entity == null)
            return;
        if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var entityAssets))
            return;
        var unitLabel = Instantiate(_unitLabelPrefab, _container.transform);
        unitLabel.SetName(entity.Id);
        _unitLabel = unitLabel;
        _labelUid = entity.Uid;
        unitLabel.HealthCounter.SetOwner(entity, _gridManager.LocalSeat);
        RefreshLabelHealth();
        
        if (!entity.TryGetComponent<AbilityComponent>(out var abilities))
            return;
        Log($"Selection info: {entity.Id}, abilities={abilities.List.Count}");
        var seat = _gridManager.LocalSeat;
        var abilityIcons = entityAssets.AbilityIcons;
        for (var i = 0; i < abilities.List.Count; i++)
        {
            if (abilityIcons.Count <= i)
                break;
            var icon = Instantiate(_abilityIconPrefab, _container.transform);
            icon.Init(abilityIcons[i], abilities.List[i], source);
            icon.OnPreviewRequested  += _gridManager.Player.OnAbilityPreview;
            icon.OnPreviewCancelled  += _gridManager.Player.OnAbilityCancelPreview;
            icon.OnActivateRequested += _gridManager.Player.OnAbilityActivate;
            icon.SetInteractable(source.Grid.CanCastAt(source.Position, abilities.List[i], seat));
            _icons.Add(icon);
        }
        var activeId = _gridManager.Player.ActiveAbilityId;
        foreach (var icon in _icons) icon.SetActiveVisual(icon.AbilityId == activeId);
    }

    private void RefreshLabelHealth()
    {
        if (_unitLabel == null) return;
        var real = _gridManager.GridState.GetEntityByUid(_labelUid);
        if (real == null || !real.TryGetComponent<HealthComponent>(out var health)) return;
        var shown = _gridManager.DisplayedState.GetEntityByUid(_labelUid);
        var next = shown != null && shown.TryGetComponent<HealthComponent>(out var shownHealth) ? shownHealth.Current : 0;
        _unitLabel.HealthCounter.SetHealthCount(next);
        _unitLabel.HealthCounter.SetPrevious(health.Current);
        _unitLabel.HealthCounter.SetDead(shown == null);
    }

    private void ClearMenu()
    {
        _unitLabel = null;
        _icons.Clear();
        UnityUtil.DestroyAllChildren(_container.gameObject);
    }
    
    // private void EventManager_OnSelectUnit(object sender, EventManager.OnSelectUnitEventArgs e)
    // {
    //     DisplayUnitInfo(e.Unit);
    // }
    //
    // private void DisplayUnitInfo(UnitManager unit)
    // {
    //
    // }
}