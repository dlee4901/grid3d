using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class EntityRenderer : LoggableBehaviour
{
    private sealed class EntityView
    {
        public GameObject Model;
        public HealthCounter Counter;
        public Entity Entity;
        public bool Shown;
        public float Height;
    }

    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GridRenderer _gridRenderer;
    [SerializeField] private HealthCounter _healthCounterPrefab;
    [SerializeField] private float _overlayHeight = 1.5f;
    [SerializeField] private int _overlaySortingOrder = -1;
    [SerializeField] private float _referenceDistance = 10f;
    [SerializeField] private float _minScale = 0.6f;

    private readonly Dictionary<int, EntityView> _views = new();
    private Canvas _overlayCanvas;
    private Camera _camera;
    private bool _showHealthCounters;

    private GridState Displayed => _gridManager.DisplayedState;

    public bool ShowHealthCounters
    {
        get => _showHealthCounters;
        set
        {
            if (_showHealthCounters == value) return;
            _showHealthCounters = value;
            foreach (var view in _views.Values) ApplyCounterVisibility(view);
        }
    }

    public void SetHealthCounterVisible(GridPosition position, bool visible)
    {
        var entity = Displayed.GetEntity(position);
        if (entity == null || !_views.TryGetValue(entity.Uid, out var view)) return;
        view.Counter.ForceVisible = visible;
        ApplyCounterVisibility(view);
    }

    public void ClearHealthCounterOverrides()
    {
        foreach (var view in _views.Values)
        {
            view.Counter.ForceVisible = false;
            ApplyCounterVisibility(view);
        }
    }

    public void Build()
    {
        _camera = _gridManager.MainCamera;
        _overlayCanvas = CreateOverlayCanvas();
        RefreshEntities();
        _gridManager.DisplayChanged += RefreshEntities;
        RenderPipelineManager.beginCameraRendering += ProjectCounters;
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= ProjectCounters;
        if (_gridManager != null) _gridManager.DisplayChanged -= RefreshEntities;
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
        foreach (var view in _views.Values)
        {
            if (!view.Counter.gameObject.activeSelf) continue;
            var world = view.Model.transform.position + Vector3.up * (view.Height + _overlayHeight);
            var screen = renderingCamera.WorldToScreenPoint(world);
            var counterTransform = (RectTransform)view.Counter.transform;
            if (screen.z <= 0f)
            {
                counterTransform.localScale = Vector3.zero;
                continue;
            }
            var steepness = Mathf.Abs(Vector3.Dot((world - renderingCamera.transform.position).normalized, Vector3.down));
            counterTransform.pivot = new Vector2(0.5f, 0.5f * steepness);
            counterTransform.position = new Vector3(screen.x, screen.y, 0f);
            counterTransform.localScale = Vector3.one * Mathf.Clamp(_referenceDistance / screen.z, _minScale, 1f);
            view.Counter.SetAlpha(view.Counter.Change != 0 ? 1f : 0.5f);
        }
    }

    public void RefreshEntities()
    {
        var shown = new HashSet<int>();
        foreach (var position in Displayed.GetOccupiedEntityPositions())
        {
            var entity = Displayed.GetEntity(position);
            if (!_views.TryGetValue(entity.Uid, out var view) && !TryCreateView(entity, out view)) continue;
            view.Entity = entity;
            view.Shown = true;
            view.Model.SetActive(true);
            view.Model.transform.position = _gridRenderer.CellCenter(position);
            if (entity.TryGetComponent<HealthComponent>(out var health))
            {
                view.Counter.SetHealthCount(health.Current);
                view.Counter.SetChange(health.Current - RealHealth(entity.Uid, health.Current));
            }
            ApplyCounterVisibility(view);
            shown.Add(entity.Uid);
        }
        foreach (var uid in _views.Keys.ToList())
        {
            if (shown.Contains(uid)) continue;
            var view = _views[uid];
            if (_gridManager.IsPreviewing)
            {
                view.Shown = false;
                view.Model.SetActive(false);
                ApplyCounterVisibility(view);
                continue;
            }
            Destroy(view.Model);
            Destroy(view.Counter.gameObject);
            _views.Remove(uid);
        }
    }

    private int RealHealth(int uid, int fallback)
    {
        if (!_gridManager.IsPreviewing) return fallback;
        var real = _gridManager.GridState.GetEntityByUid(uid);
        return real != null && real.TryGetComponent<HealthComponent>(out var health) ? health.Current : fallback;
    }

    private bool TryCreateView(Entity entity, out EntityView view)
    {
        view = null;
        if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets) || assets.Model3D == null) return false;

        var rotation = Quaternion.identity;
        if (entity.TryGetComponent<ControlComponent>(out var control) && control.PlayerController == 2)
            rotation = Quaternion.Euler(0, 180f, 0);

        view = new EntityView
        {
            Model = Instantiate(assets.Model3D, _gridRenderer.CellCenter(entity.Position), rotation, transform),
            Counter = Instantiate(_healthCounterPrefab, _overlayCanvas.transform),
            Entity = entity
        };
        view.Height = ModelHeight(view.Model);
        _views[entity.Uid] = view;
        return true;
    }

    private static float ModelHeight(GameObject model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return 0f;
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds.max.y - model.transform.position.y;
    }

    private void ApplyCounterVisibility(EntityView view)
    {
        var hasHealth = view.Entity.TryGetComponent<HealthComponent>(out HealthComponent _);
        view.Counter.gameObject.SetActive(view.Shown && hasHealth && (_showHealthCounters || view.Counter.ForceVisible || view.Counter.Change != 0));
    }
}
