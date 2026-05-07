using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillIcon : MonoBehaviour
{
    [SerializeField] private GameObject _cooldown;
    [SerializeField] private Image _icon;
    
    private InteractableUI _interactableUI;
    private Skill _skill;
    private QueryContext _ctx;
    
    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _interactableUI.OnHoverComplete += DisplaySelectablePositions;
    }
    
    public void Init(Sprite sprite, Skill skill, QueryContext ctx)
    {
        _icon.sprite = sprite;
        _skill = skill;
        _ctx = ctx;
    }
    
    private void DisplaySelectablePositions()
    {
        _skill.Selection.GetSelectablePositions(_ctx);
    }
}