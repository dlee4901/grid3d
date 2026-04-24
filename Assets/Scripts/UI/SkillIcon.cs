using TMPro;
using UnityEngine;

public class SkillIcon : MonoBehaviour
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private GameObject _cooldownOverlay;
    
    private InteractableUI _interactableUI;
    
    private void Start()
    {
        _interactableUI = UnityUtil.GetOrAddComponent<InteractableUI>(gameObject);
    }
}