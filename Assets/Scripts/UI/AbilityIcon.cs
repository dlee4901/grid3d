using System;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIcon : LoggableBehaviour
{
    [SerializeField] private GameObject _cooldown;
    [SerializeField] private Image _icon;

    private InteractableUI _interactableUI;
    private Ability _ability;
    private QueryContext _ctx;

    public event Action<Ability, QueryContext> OnPreviewRequested;
    public event Action OnPreviewCancelled;
    public event Action<Ability, QueryContext> OnActivateRequested;

    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
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
}
