using System;
using UnityEngine;
using UnityEngine.UI;

public class ManaContainer : MonoBehaviour
{
    private HorizontalLayoutGroup _container;
    private int _maxManaCount;
    
    void Start()
    {
        _container = GetComponent<HorizontalLayoutGroup>();
        _maxManaCount = _container.transform.childCount;
    }
    
    public void SetManaCount(int manaCount)
    {
        for (var i = 0; i < _maxManaCount; i++)
            _container.transform.GetChild(i).gameObject.SetActive(i < manaCount);
    }
    
    public void SetSize(float size)
    {
        for (var i = 0; i < _maxManaCount; i++)
            _container.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
    }
}