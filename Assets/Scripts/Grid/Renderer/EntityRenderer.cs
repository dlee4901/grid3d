using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EntityRenderer : LoggableBehaviour
{
    private sealed class EntityView
    {
        public GameObject Model;
        public EntityModel Visual;
        public Health Counter;
        public Entity Entity;
        public bool Shown;
        public float Height;
        public GridPosition Cell;
        public bool PreviewMoved;
        public readonly Queue<Vector3> Waypoints = new();
        public float MoveStartTime;
        public Quaternion? CastFacing;
        public Quaternion? Aim;
        public float TurnVelocity;
        public Vector3 Guide;
        public Vector3 MoveVelocity;
    }

    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GridRenderer _gridRenderer;
    [SerializeField] private Health _healthPrefab;
    [SerializeField] private float _overlayHeight = 1.5f;
    [SerializeField] private int _overlaySortingOrder = -1;
    [SerializeField] private float _referenceDistance = 10f;
    [SerializeField] private float _minScale = 0.6f;
    [SerializeField] private bool _showHealthCounters = true;
    [SerializeField] private float _pulseStrength = 0.35f;
    [SerializeField] private float _pulseFrequency = 1f;
    [SerializeField] private float _moveSpeed = 12f;
    [SerializeField] private float _moveSmoothTime = 0.08f;
    [SerializeField] private float _previewMoveDelay = 1f;
    [SerializeField] private float _turnSmoothTime = 0.12f;

    private readonly Dictionary<int, EntityView> _views = new();
    private Canvas _overlayCanvas;
    private Camera _camera;
    private int _hiddenUid = -1;
    private int _aimUid = -1;
    private Quaternion _aimRotation;

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

    public void HideModel(GridPosition? position)
    {
        _hiddenUid = position is { } cell && Displayed.GetEntity(cell) is { } entity ? entity.Uid : -1;
        RefreshEntities();
    }

    public void Build()
    {
        _camera = _gridManager.MainCamera;
        _overlayCanvas = CreateOverlayCanvas();
        RefreshEntities();
        _gridManager.DisplayChanged += RefreshEntities;
        _gridManager.Restarted += ClearViews;
        _gridManager.CommandApplied += PlayCast;
        RenderPipelineManager.beginCameraRendering += ProjectCounters;
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= ProjectCounters;
        if (_gridManager != null)
        {
            _gridManager.DisplayChanged -= RefreshEntities;
            _gridManager.Restarted -= ClearViews;
            _gridManager.CommandApplied -= PlayCast;
        }
        if (_overlayCanvas != null) Destroy(_overlayCanvas.gameObject);
    }

    private Canvas CreateOverlayCanvas()
    {
        var canvas = new GameObject("EntityOverlayCanvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = _overlaySortingOrder;
        var scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0f;
        return canvas;
    }

    private void ProjectCounters(ScriptableRenderContext context, Camera renderingCamera)
    {
        if (renderingCamera != _camera) return;
        var pulse = Pulse();
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
            view.Counter.SetPulse(view.Counter.Change != 0 ? pulse : 0f);
            view.Counter.SetAlpha(view.Counter.Change != 0 ? 1f : 0.5f);
        }
    }

    private void Update()
    {
        var step = _moveSpeed * Time.deltaTime;
        foreach (var view in _views.Values)
        {
            if (HasStarted(view))
            {
                view.Guide = Vector3.MoveTowards(view.Guide, view.Waypoints.Peek(), step);
                if (view.Guide == view.Waypoints.Peek()) view.Waypoints.Dequeue();
            }
            Follow(view);
            if (view.Visual != null) view.Visual.SetMoving(IsMoving(view));
            if (view.CastFacing != null && (view.Visual == null || !view.Visual.IsActing)) view.CastFacing = null;
            Turn(view);
        }
    }

    private void PlayCast(Command command)
    {
        if (command is not AbilityCommand cast || !IdRegistry<AbilityVisuals>.TryGet(cast.AbilityId, out var visuals)) return;
        var caster = _gridManager.GridState.GetEntity(cast.SourcePosition);
        if (caster == null || !_views.TryGetValue(caster.Uid, out var view) || view.Visual == null) return;
        view.Visual.Play(visuals.CasterAction);
        if (TryAim(cast, out _, out var rotation)) view.CastFacing = rotation;
    }

    private bool TryAim(AbilityCommand cast, out Entity caster, out Quaternion rotation)
    {
        rotation = Quaternion.identity;
        caster = _gridManager.GridState.GetEntity(cast.SourcePosition);
        if (caster == null || cast.TargetPositions.Length == 0) return false;
        if (!IdRegistry<AbilityConfig>.TryGet(cast.AbilityId, out var config) || config.Targeting is not DirectionTargeting) return false;
        var target = new GridPosition(_gridManager.GridState, cast.TargetPositions[0]);
        var offset = _gridRenderer.CellCenter(target) - _gridRenderer.CellCenter(caster.Position);
        offset.y = 0f;
        if (offset == Vector3.zero) return false;
        rotation = Quaternion.LookRotation(offset);
        return true;
    }

    private void UpdateAim()
    {
        _aimUid = -1;
        if (_gridManager.PreviewCommand is not AbilityCommand cast) return;
        if (!TryAim(cast, out var caster, out _aimRotation)) return;
        _aimUid = caster.Uid;
    }

    private void Turn(EntityView view)
    {
        var model = view.Model.transform;
        var current = model.eulerAngles.y;
        var wanted = WantedRotation(view).eulerAngles.y;
        var yaw = Mathf.SmoothDampAngle(current, wanted, ref view.TurnVelocity, _turnSmoothTime);
        model.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    private Quaternion WantedRotation(EntityView view)
    {
        if (view.CastFacing != null) return view.CastFacing.Value;
        if (IsMoving(view))
        {
            var ahead = HasStarted(view) ? view.Waypoints.Peek() : view.Guide;
            var heading = ahead - view.Model.transform.position;
            heading.y = 0f;
            if (heading != Vector3.zero) return Quaternion.LookRotation(heading);
        }
        if (view.Aim != null) return view.Aim.Value;
        return RotationOf(view.Entity.Facing);
    }

    private const float SettleDistance = 0.01f;

    private static bool HasStarted(EntityView view) => view.Waypoints.Count > 0 && Time.time >= view.MoveStartTime;

    private static bool IsMoving(EntityView view)
        => HasStarted(view) || (view.Model.transform.position - view.Guide).sqrMagnitude > SettleDistance * SettleDistance;

    private void Follow(EntityView view)
    {
        var model = view.Model.transform;
        if (!IsMoving(view))
        {
            model.position = view.Guide;
            view.MoveVelocity = Vector3.zero;
            return;
        }
        model.position = Vector3.SmoothDamp(model.position, view.Guide, ref view.MoveVelocity, _moveSmoothTime);
    }

    private void Place(EntityView view, Vector3 position)
    {
        view.Model.transform.position = position;
        view.Guide = position;
        view.MoveVelocity = Vector3.zero;
    }

    private void MoveTo(EntityView view, GridPosition cell)
    {
        var previewing = _gridManager.IsPreviewing;
        if (!previewing) view.MoveStartTime = 0f;
        if (view.Cell.Equals(cell))
        {
            if (!previewing) view.PreviewMoved = false;
            return;
        }
        if (TeleportedTo(view, cell))
        {
            view.Cell = cell;
            view.Waypoints.Clear();
            view.PreviewMoved = previewing;
            Place(view, _gridRenderer.CellCenter(cell));
            return;
        }
        var movePath = PathFromMoves(view, cell);
        view.Cell = cell;
        view.Waypoints.Clear();
        if (view.PreviewMoved && (!previewing || movePath == null))
        {
            view.PreviewMoved = false;
            Place(view, _gridRenderer.CellCenter(cell));
            return;
        }
        var path = movePath ?? new List<GridPosition> { cell };
        if (movePath != null && previewing)
        {
            view.PreviewMoved = true;
            Place(view, _gridRenderer.CellCenter(movePath[0]));
            view.MoveStartTime = Time.time + _previewMoveDelay;
        }
        foreach (var step in movePath == null ? path : path.Skip(1)) view.Waypoints.Enqueue(_gridRenderer.CellCenter(step));
    }

    private bool TeleportedTo(EntityView view, GridPosition cell)
        => Displayed.Moves.Any(move => move.Uid == view.Entity.Uid && move.Teleport && move.To.Equals(cell));

    private List<GridPosition> PathFromMoves(EntityView view, GridPosition cell)
    {
        var moves = Displayed.Moves.Where(move => move.Uid == view.Entity.Uid).ToList();
        if (moves.Count == 0 || !moves[^1].To.Equals(cell)) return null;
        if (!_gridManager.IsPreviewing && !moves[0].From.Equals(view.Cell)) return null;
        var path = new List<GridPosition> { moves[0].From };
        path.AddRange(moves.Select(move => move.To));
        return path;
    }

    private float Pulse() => _pulseStrength * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * _pulseFrequency * 2f * Mathf.PI));

    private void ClearViews()
    {
        _hiddenUid = -1;
        foreach (var view in _views.Values)
        {
            Destroy(view.Model);
            Destroy(view.Counter.gameObject);
        }
        _views.Clear();
    }

    public void RefreshEntities()
    {
        var shown = new HashSet<int>();
        UpdateAim();
        foreach (var position in Displayed.GetOccupiedEntityPositions())
        {
            var entity = Displayed.GetEntity(position);
            if (!_views.TryGetValue(entity.Uid, out var view) && !TryCreateView(entity, out view)) continue;
            view.Entity = entity;
            view.Shown = entity.Uid != _hiddenUid;
            view.Model.SetActive(view.Shown);
            view.Aim = entity.Uid == _aimUid ? _aimRotation : null;
            MoveTo(view, position);
            if (entity.TryGetComponent<HealthComponent>(out var health))
            {
                view.Counter.SetHealthCount(health.Current);
                view.Counter.SetOwner(entity, _gridManager.ViewerSeat);
                view.Counter.SetPrevious(RealHealth(entity.Uid, health.Current));
                view.Counter.SetDead(false);
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
                var real = _gridManager.GridState.GetEntityByUid(uid);
                view.Shown = real != null;
                view.Model.SetActive(view.Shown);
                if (view.Shown) ShowDeath(view, real);
                ApplyCounterVisibility(view);
                continue;
            }
            Destroy(view.Model);
            Destroy(view.Counter.gameObject);
            _views.Remove(uid);
        }
    }

    private void ShowDeath(EntityView view, Entity real)
    {
        view.Entity = real;
        MoveTo(view, real.Position);
        if (!real.TryGetComponent<HealthComponent>(out var health)) return;
        view.Counter.SetHealthCount(0);
        view.Counter.SetPrevious(health.Current);
        view.Counter.SetDead(true);
    }

    private int RealHealth(int uid, int fallback)
    {
        if (!_gridManager.IsPreviewing) return fallback;
        var real = _gridManager.GridState.GetEntityByUid(uid);
        return real != null && real.TryGetComponent<HealthComponent>(out var health) ? health.Current : fallback;
    }

    public static Quaternion RotationOf(GridDirectionFacing facing) => Quaternion.Euler(0f, 90f * (int)facing, 0f);

    private bool TryCreateView(Entity entity, out EntityView view)
    {
        view = null;
        if (!IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets) || assets.Model3D == null) return false;

        var rotation = RotationOf(entity.Facing);
        view = new EntityView
        {
            Model = Instantiate(assets.Model3D, _gridRenderer.CellCenter(entity.Position), rotation, transform),
            Counter = Instantiate(_healthPrefab, _overlayCanvas.transform),
            Entity = entity,
            Cell = entity.Position
        };
        view.Guide = view.Model.transform.position;
        if (!view.Model.TryGetComponent(out view.Visual)) LogError($"Model for '{entity.Id}' has no EntityModel component");
        view.Height = ModelHeight(view.Model);
        _views[entity.Uid] = view;
        return true;
    }

    public static float ModelHeight(GameObject model)
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
