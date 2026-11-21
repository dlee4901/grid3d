using System;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionOrderIcon : LoggableBehaviour
{
    [SerializeField] private RectTransform _visual;
    [SerializeField] private float _slideDuration = 0.5f;
    [SerializeField] private UnitPortrait _portrait;
    [SerializeField] private int _dragSortingOrder = 10;
    
    public Entity Entity { get; private set; }
    public event Action<Entity> OnSelectRequested;
    
    private InteractableUI _interactableUI;
    private float _slideSpeed;
    private bool _dragging;
    private Canvas _dragCanvas;
    
    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _interactableUI.OnClickCompleted += () => OnSelectRequested?.Invoke(Entity);
    }
    
    public void Bind(Entity entity, int viewerPlayer)
    {
        Entity = entity;
        var sprite = IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets) ? assets.Model2D : null;
        var hasHealth = entity.TryGetComponent<HealthComponent>(out var health);
        _portrait.Init(sprite, 0, hasHealth);
        if (!hasHealth) return;
        _portrait.Health.SetHealthCount(health.Current);
        _portrait.Health.SetOwner(entity, viewerPlayer);
    }

    public void SetBorderColor(Color color) => _portrait.SetBorderColor(color);

    public void SlideFrom(float offset)
    {
        _visual.anchoredPosition = new Vector2(offset, 0f);
        _slideSpeed = Mathf.Abs(offset) / _slideDuration;
    }

    public void BeginDrag()
    {
        _dragging = true;
        _dragCanvas = gameObject.AddComponent<Canvas>();
        _dragCanvas.overrideSorting = true;
        _dragCanvas.sortingOrder = _dragSortingOrder;
    }

    public void DragTo(float offset) => _visual.anchoredPosition = new Vector2(offset, 0f);

    public void EndDrag()
    {
        _dragging = false;
        Destroy(_dragCanvas);
        SlideFrom(_visual.anchoredPosition.x);
    }

    private void Update()
    {
        if (_dragging) return;
        var x = _visual.anchoredPosition.x;
        if (x == 0f) return;
        _visual.anchoredPosition = new Vector2(Mathf.MoveTowards(x, 0f, _slideSpeed * Time.deltaTime), 0f);
    }
}