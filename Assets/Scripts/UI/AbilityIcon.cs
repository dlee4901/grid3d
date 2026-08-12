using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIcon : LoggableBehaviour
{
    private enum ManaCounterPosition { North, East, South, West }
    
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _cooldown;
    [SerializeField] private GameObject _activeHighlight;
    [SerializeField] private Color _disabledTint = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private ManaCounter _manaCounter;
    [SerializeField] private ManaCounterPosition _manaCounterPosition = ManaCounterPosition.East;
    [SerializeField] private float _manaCounterTransformOffset = 64f;

    private InteractableUI _interactableUI;
    private TextMeshPro _cooldownText;
    private TextMeshPro _manaCostText;
    private RectTransform _manaCounterTransform;
    
    private Ability _ability;
    private GridSource _source;

    public string AbilityId => _ability?.Id;

    public event Action<Ability, GridSource> OnPreviewRequested;
    public event Action OnPreviewCancelled;
    public event Action<Ability, GridSource> OnActivateRequested;

    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _cooldownText = _cooldown.GetComponentInChildren<TextMeshPro>();
        _manaCounterTransform = _manaCounter.GetComponent<RectTransform>();
        SetManaCounterPosition();
        
        _interactableUI.OnHoverTriggered += () => { Log($"PreviewRequested: {_ability?.Id}"); OnPreviewRequested?.Invoke(_ability, _source); };
        _interactableUI.OnHoverCompleted     += () => { Log("PreviewCancelled"); OnPreviewCancelled?.Invoke(); };
        
        _interactableUI.OnHoldTriggered += () => { Log($"PreviewRequested: {_ability?.Id}"); OnPreviewRequested?.Invoke(_ability, _source); };
        _interactableUI.OnHoldCompleted     += () => { Log("PreviewCancelled"); OnPreviewCancelled?.Invoke(); };
        
        _interactableUI.OnClickCompleted += () => { Log($"ActivateRequested: {_ability?.Id}"); OnActivateRequested?.Invoke(_ability, _source); };
    }

    public void Init(Sprite sprite, Ability ability, GridSource source)
    {
        _icon.sprite = sprite;
        _ability = ability;
        _source = source;
        UpdateInfo();
    }

    public void SetActiveVisual(bool active)
    {
        if (_activeHighlight != null) _activeHighlight.SetActive(active);
    }

    public void SetInteractable(bool interactable)
    {
        if (_interactableUI == null) _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _interactableUI.enabled = interactable;
        _icon.color = interactable ? Color.white : _disabledTint;
    }
    
    public void UpdateInfo()
    {
        if (_ability.Cooldown > 0)
        {
            _cooldown.SetActive(true);
            _cooldownText.text = _ability.Cooldown.ToString();
            _manaCounter.gameObject.SetActive(false);
        }
        else
        {
            _cooldown.SetActive(false);
            _manaCounter.gameObject.SetActive(true);
            _manaCounter.SetManaCount(_ability.ManaCost);
        }
    }
    
    private void SetManaCounterPosition()
    {
        switch (_manaCounterPosition)
        {
            case ManaCounterPosition.North:
                UnityUtil.SetRectMargins(_manaCounterTransform, 0, 0, 0, _manaCounterTransformOffset);
                break;
            case ManaCounterPosition.East:
                UnityUtil.SetRectMargins(_manaCounterTransform, _manaCounterTransformOffset, 0, 0, 0);
                break;
            case ManaCounterPosition.West:
                UnityUtil.SetRectMargins(_manaCounterTransform, 0, 0, _manaCounterTransformOffset, 0);
                break;
            case ManaCounterPosition.South:
                UnityUtil.SetRectMargins(_manaCounterTransform, 0, _manaCounterTransformOffset, 0, 0);
                break;
            default:
                UnityUtil.SetRectMargins(_manaCounterTransform, _manaCounterTransformOffset, 0, 0, 0);
                break;
        }
    }
}
