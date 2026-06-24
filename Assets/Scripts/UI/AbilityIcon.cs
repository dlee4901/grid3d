using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIcon : LoggableBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _manaCost;
    [SerializeField] private GameObject _cooldown;

    private InteractableUI _interactableUI;
    private TextMeshPro _cooldownText;
    private TextMeshPro _manaCostText;
    
    private Ability _ability;
    private QueryContext _ctx;

    public event Action<Ability, QueryContext> OnPreviewRequested;
    public event Action OnPreviewCancelled;
    public event Action<Ability, QueryContext> OnActivateRequested;

    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _cooldownText = _cooldown.GetComponentInChildren<TextMeshPro>();
        _manaCostText = _manaCost.GetComponentInChildren<TextMeshPro>();
        _interactableUI.OnHoverTriggered += () => { Log($"PreviewRequested: {_ability?.Id}"); OnPreviewRequested?.Invoke(_ability, _ctx); };
        _interactableUI.OnHoverEnded     += () => { Log("PreviewCancelled"); OnPreviewCancelled?.Invoke(); };
        _interactableUI.OnClickCompleted += () => { Log($"ActivateRequested: {_ability?.Id}"); OnActivateRequested?.Invoke(_ability, _ctx); };
    }

    public void Init(Sprite sprite, Ability ability, QueryContext ctx)
    {
        _icon.sprite = sprite;
        _ability = ability;
        _ctx = ctx;
    }
    
    public void UpdateInfo()
    {
        if (_ability.Cooldown > 0)
        {
            _manaCost.SetActive(false);
            _cooldown.SetActive(true);
            _cooldownText.text = _ability.Cooldown.ToString();
        }
        else
        {
            _cooldown.SetActive(false);
            _manaCost.SetActive(true);
            _manaCostText.text = _ability.ManaCost.ToString();
        }
    }
}
