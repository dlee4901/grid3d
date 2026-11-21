using UnityEngine;

public class FontShowcase : LoggableBehaviour
{
    [SerializeField] private int _heartNumber = 10;
    [SerializeField] private int _counterNumber = 99;

#if UNITY_EDITOR
    private void OnValidate() => UnityEditor.EditorApplication.delayCall += Apply;
#endif

    private void Apply()
    {
        if (this == null) return;
        foreach (var heart in GetComponentsInChildren<Health>(true)) heart.SetHealthCount(_heartNumber);
        foreach (var counter in GetComponentsInChildren<Counter>(true)) counter.SetValue(_counterNumber);
    }
}
