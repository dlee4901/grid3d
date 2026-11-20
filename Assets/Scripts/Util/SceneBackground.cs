using UnityEngine;

[ExecuteAlways]
public class SceneBackground : MonoBehaviour
{
    [SerializeField] private Material[] _skyboxes;
    [SerializeField] private int _selected;

    private void OnEnable() => Apply();

    private void OnValidate() => Apply();

    private void Apply()
    {
        if (_skyboxes == null || _skyboxes.Length == 0) return;
        _selected = Mathf.Clamp(_selected, 0, _skyboxes.Length - 1);
        var skybox = _skyboxes[_selected];
        if (skybox == null || RenderSettings.skybox == skybox) return;
        RenderSettings.skybox = skybox;
    }
}
