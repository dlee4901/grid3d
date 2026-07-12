using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManaCounter : MonoBehaviour
{
    [SerializeField] private Image _manaIcon;
    [SerializeField] private TMP_Text _counterText;
    [SerializeField] private Outline _outline;
    
    [SerializeField] private float _size = 64f;
    
    private float _linePosition;
    private float _cornerPosition;
    private float _lineLength;
    private float _fontSize;
    
    private RectTransform _manaIconTransform;
    private RectTransform _counterTextTransform;
    
    private void Start()
    {
        _manaIconTransform = _manaIcon.gameObject.GetComponent<RectTransform>();
        _counterTextTransform = _counterText.GetComponent<RectTransform>();
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(_size, _size);
        InitOutline();
        
        _counterText.fontSize = _fontSize;
    }
    
    private void InitOutline()
    {
        _linePosition = (_size - 8f) / 4f;
        _cornerPosition = _linePosition * 2f;
        _lineLength = _cornerPosition * Mathf.Sqrt(2);
        _fontSize = _size / 2f;
        _outline.SetTransformsDiamond(_linePosition, _cornerPosition, _lineLength);
    }
    
    public void SetManaCount(int manaCount)
    {
        _counterText.text = manaCount.ToString();
    }
    
    public void ToggleOutline(bool on)
    {
        _outline.gameObject.SetActive(on);
    }
}