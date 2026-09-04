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
    
    [SerializeField] private Color _allyColor = new Color32(255, 255, 32, 255);
    [SerializeField] private Color _neutralColor = new Color32(255, 128, 32, 255);
    [SerializeField] private Color _enemyColor = new Color32(255, 32, 32, 255);
    
    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _interactableUI.OnClickCompleted += () => OnSelectRequested?.Invoke(Entity);
        _image.preserveAspect = true;
    }
    
    public void Bind(IReadOnlyEntity entity, int viewerPlayer)
    {
        Entity = entity;
        _image.sprite = IdRegistry<EntityAssets>.TryGet(entity.Id, out var assets) ? assets.Model2D : null;
        var hasHealth = entity.TryGetComponent<HealthComponent>(out var health);
        _healthCounter.gameObject.SetActive(hasHealth);
        if (!hasHealth) return;
        _healthCounter.SetHealthCount(health.Current);
        _healthCounter.SetColor(HeartColor(entity, viewerPlayer));
    }

    private Color HeartColor(IReadOnlyEntity entity, int viewerPlayer)
    {
        if (!entity.TryGetComponent<ControlComponent>(out var control) || control.PlayerController == 0)
            return _neutralColor;
        return control.PlayerController == viewerPlayer ? _allyColor : _enemyColor;
    }

    public void SetOutlineColor(Color color) => _outline.SetColor(color);
}