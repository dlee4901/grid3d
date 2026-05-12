using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillIcon : MonoBehaviour
{
    [SerializeField] private GameObject _cooldown;
    
    private InteractableUI _interactableUI;
    private Image _icon;
    
    private Skill _skill;
    
    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
        _interactableUI.OnHoverComplete += DisplaySelectablePositions;
    }
    
    public void DisplaySelectablePositions()
    {
        //_skill.Selection.GetSelectablePositions()
    }
    
    public void Init(Sprite sprite, Skill skill)
    {
        _icon.sprite = sprite;
        _skill = skill;
    }
}