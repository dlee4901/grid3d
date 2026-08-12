using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EntityRenderer : LoggableBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GridRenderer _gridRenderer;
    [SerializeField] private HealthCounter _healthCounterPrefab;
    [SerializeField] private float _overlayHeight = 1.5f;
    [SerializeField] private int _overlaySortingOrder = -1;

    private GameObject[] _entityModels;
    private HealthCounter[] _healthCounters;
    private Canvas _overlayCanvas;
    private Camera _camera;
    private bool _showHealthCounters;

    private IReadOnlyGridState GridState => _gridManager.GridState;

    public bool ShowHealthCounters
    {
        get => _showHealthCounters;
        set
        {
            if (_showHealthCounters == value) return;
            _showHealthCounters = value;
            RefreshCounterVisibility();
        }
    }

    public void SetHealthCounterVisible(GridPosition position, bool visible)
    {
        if (!position.IsValid()) return;
        var counter = _healthCounters[position.Dim1];
        if (counter == null) return;
        counter.ForceVisible = visible;
        ApplyCounterVisibility(position.Dim1);
    }

    public void ClearHealthCounterOverrides()
    {
        for (var i = 0; i < _healthCounters.Length; i++)
        {
            if (_healthCounters[i] == null) continue;
            _healthCounters[i].ForceVisible = false;
            ApplyCounterVisibility(i);
        }
    }

    public void Build()
    {
        _camera = _gridManager.MainCamera;
        _overlayCanvas = CreateOverlayCanvas();
        _entityModels = new GameObject[GridState.Size];
        _healthCounters = new HealthCounter[GridState.Size];
        foreach (var position in GridState.GetOccupiedEntityPositions())
            CreateEntity(GridState.GetEntity(position), position);
        _gridManager.StateChanged += RefreshEntities;
        RenderPipelineManager.beginCameraRendering += ProjectCounters;
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= ProjectCounters;
        if (_gridManager != null) _gridManager.StateChanged -= RefreshEntities;
        if (_overlayCanvas != null) Destroy(_overlayCanvas.gameObject);
    }

    private Canvas CreateOverlayCanvas()
    {
        var canvas = new GameObject("EntityOverlayCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = _overlaySortingOrder;
        return canvas;
    }

    private void ProjectCounters(ScriptableRenderContext context, Camera renderingCamera)
    {
        if (renderingCamera != _camera) return;
        for (var i = 0; i < _healthCounters.Length; i++)
        {
            var counter = _healthCounters[i];
            if (counter == null || !counter.gameObject.activeSelf) continue;
            var world = _gridRenderer.CellCenter(new GridPosition(GridState, i), _overlayHeight);
            var screen = renderingCamera.WorldToScreenPoint(world);
            counter.transform.position = new Vector3(screen.x, screen.y, 0f);
            counter.SetAlpha(0.5f);
        }
    }

    private void CreateEntity(Entity entity, GridPosition position)
    {
        if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets)) return;
        if (assets.Model3D == null) return;

        var rotation = Quaternion.identity;
        if (entity.TryGetComponent<ControlComponent>(out ControlComponent control)
            && control.PlayerController == 2)
            rotation = Quaternion.Euler(0, 180f, 0);

        _entityModels[position.Dim1] =
            Instantiate(assets.Model3D, _gridRenderer.CellCenter(position), rotation, transform);

        var counter = Instantiate(_healthCounterPrefab, _overlayCanvas.transform);
        _healthCounters[position.Dim1] = counter;

        RefreshCounter(entity, counter);
    }

    public void RefreshEntities()
    {
        var carried = new Dictionary<Entity, (GameObject model, HealthCounter counter)>();
        for (var i = 0; i < _entityModels.Length; i++)
        {
            if (_entityModels[i] == null) continue;
            var entity = GridState.GetEntity(i);
            if (entity == null)
            {
                Destroy(_entityModels[i]);
                Destroy(_healthCounters[i].gameObject);
                _entityModels[i] = null;
                _healthCounters[i] = null;
                continue;
            }
            carried[entity] = (_entityModels[i], _healthCounters[i]);
            _entityModels[i] = null;
            _healthCounters[i] = null;
        }
        foreach (var (entity, carriedEntity) in carried)
        {
            var position = entity.Position;
            var (model, counter) = carriedEntity;
            model.transform.position = _gridRenderer.CellCenter(position);
            _entityModels[position.Dim1] = model;
            _healthCounters[position.Dim1] = counter;
            RefreshCounter(entity, counter);
        }
        foreach (var position in GridState.GetOccupiedEntityPositions())
        {
            if (_entityModels[position.Dim1] != null) continue;
            CreateEntity(GridState.GetEntity(position), position);
        }
    }

    private void RefreshCounter(IReadOnlyEntity entity, HealthCounter counter)
    {
        if (entity.TryGetComponent<HealthComponent>(out HealthComponent health))
            counter.SetHealthCount(health.Current);
        ApplyCounterVisibility(entity.Position.Dim1);
    }

    private void RefreshCounterVisibility()
    {
        for (var i = 0; i < _healthCounters.Length; i++) ApplyCounterVisibility(i);
    }

    private void ApplyCounterVisibility(int index)
    {
        var counter = _healthCounters[index];
        if (counter == null) return;
        var entity = GridState.GetEntity(index);
        var hasHealth = entity != null && entity.TryGetComponent<HealthComponent>(out HealthComponent _);
        counter.gameObject.SetActive(hasHealth && (_showHealthCounters || counter.ForceVisible));
    }
}
