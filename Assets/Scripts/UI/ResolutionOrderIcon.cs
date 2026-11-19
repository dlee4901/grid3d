using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResolutionOrderIcon : LoggableBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform _visual;
    [SerializeField] private Image _border;
    [SerializeField] private float _slideDuration = 0.5f;
    [SerializeField] private Image _image;
    [SerializeField] private HealthCounter _healthCounter;
    
    public Entity Entity { get; private set; }
    public event Action<Entity> OnSelectRequested;
    public event Action<Entity> OnHoverChanged;
    
    private InteractableUI _interactableUI;
    private float _slideSpeed;
    
    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _interactableUI.OnClickCompleted += () => OnSelectRequested?.Invoke(Entity);
        _image.preserveAspect = true;
    }
    
    public void Bind(Entity entity, int viewerPlayer)
    {
        Entity = entity;
        _image.sprite = IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets) ? assets.Model2D : null;
        var hasHealth = entity.TryGetComponent<HealthComponent>(out var health);
        _healthCounter.gameObject.SetActive(hasHealth);
        if (!hasHealth) return;
        _healthCounter.SetHealthCount(health.Current);
        _healthCounter.SetOwner(entity, viewerPlayer);
    }

    public void OnPointerEnter(PointerEventData eventData) => OnHoverChanged?.Invoke(Entity);

    public void OnPointerExit(PointerEventData eventData) => OnHoverChanged?.Invoke(null);

    public void SetBorderColor(Color color) => _border.color = color;

    public void SlideFrom(float offset)
    {
        _visual.anchoredPosition = new Vector2(offset, 0f);
        _slideSpeed = Mathf.Abs(offset) / _slideDuration;
    }

    private void Update()
    {
        var x = _visual.anchoredPosition.x;
        if (x == 0f) return;
        _visual.anchoredPosition = new Vector2(Mathf.MoveTowards(x, 0f, _slideSpeed * Time.deltaTime), 0f);
    }
}