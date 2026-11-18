using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResolutionOrderIcon : LoggableBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Outline _outline;
    [SerializeField] private Image _image;
    [SerializeField] private HealthCounter _healthCounter;
    
    public Entity Entity { get; private set; }
    public event Action<Entity> OnSelectRequested;
    public event Action<Entity> OnHoverChanged;
    
    private InteractableUI _interactableUI;
    
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

    public void SetOutlineColor(Color color) => _outline.SetColor(color);
}