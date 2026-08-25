using System;
using UnityEngine;
using UnityEngine.UI;

public class ResolutionOrderIcon : LoggableBehaviour
{
    [SerializeField] private Outline _outline;
    [SerializeField] private Image _image;
    [SerializeField] private HealthCounter _healthCounter;
    
    public IReadOnlyEntity Entity { get; private set; }
    public event Action<IReadOnlyEntity> OnSelectRequested;
    
    private InteractableUI _interactableUI;
    
    private Color _activeColor = new Color32(0, 255, 0, 64);
    private Color _enemyColor = new Color32(255, 0, 0, 64);
    private Color _neutralColor = new Color32(255, 255, 0, 64);
    
    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _interactableUI.OnClickCompleted += () => OnSelectRequested?.Invoke(Entity);
        _image.preserveAspect = true;
    }
    
    public void Bind(IReadOnlyEntity entity)
    {
        Entity = entity;
        _image.sprite = IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets) ? assets.Model2D : null;
        var hasHealth = entity.TryGetComponent<HealthComponent>(out var health);
        _healthCounter.gameObject.SetActive(hasHealth);
        if (hasHealth) _healthCounter.SetHealthCount(health.Current);
    }

    public void SetOutlineColor(Color color) => _outline.SetColor(color);
}