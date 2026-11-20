using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIcon : LoggableBehaviour
{
    private enum ManaCounterPosition { North, East, South, West }
    
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _cooldown;
    [SerializeField] private TMP_Text _cooldownText;
    [SerializeField] private GameObject _activeHighlight;
    [SerializeField] private Color _disabledTint = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Image _border;
    [SerializeField] private Color32 _borderColor = new(255, 255, 255, 64);
    [SerializeField] private Color32 _activeBorderColor = new(255, 192, 64, 255);
    [SerializeField] private ManaCounter _manaCounter;
    [SerializeField] private ManaCounterPosition _manaCounterPosition = ManaCounterPosition.East;
    [SerializeField] private float _manaCounterTransformOffset = 64f;

    private InteractableUI _interactableUI;
    private TextMeshPro _manaCostText;
    private RectTransform _manaCounterTransform;
    
    private Ability _ability;
    private Trait _trait;
    private Status _status;
    private RawImage _textureImage;
    private GridSource _source;

    public string AbilityId => _ability?.Id;
    public Ability Ability => _ability;
    public Trait Trait => _trait;
    public Status Status => _status;
    public GridSource Source => _source;

    public event Action<AbilityIcon> OnClicked;

    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _manaCounterTransform = _manaCounter.GetComponent<RectTransform>();
        
        _interactableUI.OnClickCompleted += () => OnClicked?.Invoke(this);
    }

    public void Init(Sprite sprite, Ability ability, GridSource source)
    {
        _icon.sprite = sprite;
        _ability = ability;
        _source = source;
    }

    public void InitTrait(Sprite sprite, Trait trait)
    {
        _icon.sprite = sprite;
        _trait = trait;
        _cooldown.SetActive(false);
        _manaCounter.gameObject.SetActive(false);
    }

    public void InitStatus(Sprite sprite, int count, Status status = null)
    {
        _icon.sprite = sprite;
        _status = status;
        _cooldown.SetActive(false);
        _manaCounter.gameObject.SetActive(true);
        _manaCounter.SetManaCount(count);
    }

    public void InitSwatch(Texture texture, int turns)
    {
        if (_textureImage == null)
        {
            _textureImage = new GameObject("Texture", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
            var rect = _textureImage.rectTransform;
            rect.SetParent(_icon.rectTransform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            _textureImage.raycastTarget = false;
        }
        _textureImage.texture = texture;
        _icon.sprite = null;
        _icon.color = Color.black;
        _cooldown.SetActive(false);
        _manaCounter.gameObject.SetActive(turns > 0);
        if (turns > 0) _manaCounter.SetManaCount(turns);
    }

    public void SetActiveVisual(bool active)
    {
        if (_activeHighlight != null) _activeHighlight.SetActive(active);
        if (_border != null) _border.color = active ? _activeBorderColor : _borderColor;
    }

    public void SetCastable(bool castable) => _icon.color = castable ? Color.white : _disabledTint;
    
    public void Render(Ability shown, bool castable)
    {
        var onCooldown = shown.Cooldown > 0;
        _cooldown.SetActive(onCooldown);
        if (onCooldown) _cooldownText.text = shown.Cooldown.ToString();
        _manaCounter.gameObject.SetActive(true);
        _manaCounter.SetManaCount(shown.ManaCost);
        _manaCounter.SetDimmed(onCooldown);
        SetCastable(castable);
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
