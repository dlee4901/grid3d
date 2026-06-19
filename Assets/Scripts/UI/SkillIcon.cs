using System;
using UnityEngine;
using UnityEngine.UI;

public class SkillIcon : LoggableBehaviour
{
    [SerializeField] private GameObject _cooldown;
    [SerializeField] private Image _icon;

    private InteractableUI _interactableUI;
    private Skill _skill;
    private QueryContext _ctx;

    public event Action<Skill, QueryContext> OnPreviewRequested;
    public event Action OnPreviewCancelled;
    public event Action<Skill, QueryContext> OnActivateRequested;

    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _interactableUI.OnHoverTriggered += () => { Log($"PreviewRequested: {_skill?.Id}"); OnPreviewRequested?.Invoke(_skill, _ctx); };
        _interactableUI.OnHoverEnded     += () => { Log("PreviewCancelled"); OnPreviewCancelled?.Invoke(); };
        _interactableUI.OnClickCompleted += () => { Log($"ActivateRequested: {_skill?.Id}"); OnActivateRequested?.Invoke(_skill, _ctx); };
    }

    public void Init(Sprite sprite, Skill skill, QueryContext ctx)
    {
        _icon.sprite = sprite;
        _skill = skill;
        _ctx = ctx;
    }
}
