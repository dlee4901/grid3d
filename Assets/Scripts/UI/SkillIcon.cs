using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillIcon : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _cooldown;
    
    private InteractableUI _interactableUI;
    
    private Skill _skill;
    
    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
    }
    
    public void Init(Sprite sprite, Skill skill)
    {
        _icon.sprite = sprite;
        _skill = skill;
    }
}