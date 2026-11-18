using UnityEngine;

[CreateAssetMenu(fileName = "TeamPalette", menuName = "Grid3D/Team Palette")]
public class TeamPalette : ScriptableObject
{
    [SerializeField] private Color _ally = new Color32(40, 200, 180, 255);
    [SerializeField] private Color _neutral = new Color32(255, 128, 32, 255);
    [SerializeField] private Color _enemy = new Color32(230, 50, 60, 255);

    public Color ColorFor(Entity entity, int viewerPlayer)
    {
        if (!entity.TryGetComponent<ControlComponent>(out var control) || control.PlayerController == 0) return _neutral;
        return control.PlayerController == viewerPlayer ? _ally : _enemy;
    }
}
