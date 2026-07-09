using UnityEngine;

public class ManaDisplay : MonoBehaviour
{
    [SerializeField] private ManaContainer _manaContainer;
    [SerializeField] private ManaCounter _manaCounter;
    
    [SerializeField] private float _manaContainerSize;
    [SerializeField] private float _manaCounterSize;
    [SerializeField] private int _manaCounterTextSize;
    
    public int ManaCount { get; private set; }
    
    void Start()
    {
        _manaContainer.SetSize(_manaContainerSize);
        _manaCounter.gameObject.SetActive(true);
    }
    
    public void SetManaCount(int manaCount)
    {
        if (manaCount == ManaCount)
            return;
        _manaCounter.SetManaCount(manaCount);
        // if (manaCount > 5)
        // {
        //     _manaContainer.gameObject.SetActive(false);
        //     _manaCounter.gameObject.SetActive(true);
        //     _manaCounter.SetManaCount(manaCount);
        // }
        // else
        // {
        //     _manaCounter.gameObject.SetActive(false);
        //     _manaContainer.gameObject.SetActive(true);
        //     _manaContainer.SetManaCount(manaCount);
        // }
        ManaCount = manaCount;
    }
}