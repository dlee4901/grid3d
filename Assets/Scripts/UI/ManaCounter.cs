using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManaCounter : MonoBehaviour
{
    [SerializeField] private Image _manaIcon;
    [SerializeField] private TMP_Text _counterText;
    
    private RectTransform _manaIconTransform;
    private RectTransform _counterTextTransform;
    
    private void Start()
    {
        _manaIconTransform = _manaIcon.gameObject.GetComponent<RectTransform>();
        _counterTextTransform = _counterText.GetComponent<RectTransform>();
    }
    
    public void SetManaCount(int manaCount)
    {
        _counterText.text = manaCount.ToString();
    }
    
    public void SetSize(float size, int textSize)
    {
        _manaIconTransform.sizeDelta = new Vector2(size, size);
        _counterTextTransform.sizeDelta = new Vector2(size, size);
        _counterText.fontSize = textSize;
    }
}