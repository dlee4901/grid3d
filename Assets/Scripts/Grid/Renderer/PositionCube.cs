using UnityEngine;

public enum CubeFace { Top, Side, Bottom }
public enum HighlightType { AvailableSources, AvailableTargets, SelectionArea, EffectArea, Selected, Hover, StartingPositions }

public class PositionCube : LoggableBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private HighlightPalette _palette;
    [SerializeField] private int _overlaySlot = 1;

    [SerializeField] private Color _edgeColor = Color.white;
    [SerializeField, ColorUsage(false, true)] private Color _edgeEmission = Color.black;
    [SerializeField, ColorUsage(false, true)] private Color _topEmission = Color.black;
    [SerializeField, ColorUsage(false, true)] private Color _sideEmission = Color.black;
    [SerializeField, ColorUsage(false, true)] private Color _bottomEmission = Color.black;

    private Material _overlay;
    private TerrainAssets _terrain;
    private bool _danger;

    private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    private static readonly int EdgeEmissionId = Shader.PropertyToID("_EdgeEmission");
    private static readonly int[] FaceEmissionIds =
    {
        Shader.PropertyToID("_TopEmission"),
        Shader.PropertyToID("_SideEmission"),
        Shader.PropertyToID("_BottomEmission"),
    };

    private void Awake()
    {
        var materials = _meshRenderer.sharedMaterials;
        _overlay = new Material(materials[_overlaySlot]);
        materials[_overlaySlot] = _overlay;
        _meshRenderer.sharedMaterials = materials;
        Apply();
    }

    private void OnDestroy()
    {
        if (_overlay != null) Destroy(_overlay);
    }

    private void OnValidate()
    {
        if (Application.isPlaying && _overlay != null) Apply();
    }

    public void SetHighlight(HighlightType highlightType)
        => _overlay.SetColor(EdgeEmissionId, _palette.EdgeEmissionFor(highlightType));

    public void ClearHighlight()
        => _overlay.SetColor(EdgeEmissionId, _edgeEmission);

    public void SetTerrainEmission(TerrainAssets terrain)
    {
        _terrain = terrain;
        ApplyFaces();
    }

    public void SetDanger(bool danger)
    {
        if (_danger == danger) return;
        _danger = danger;
        ApplyFaces();
    }

    private void ApplyFaces()
    {
        var top = _terrain != null ? _terrain.TopEmission : _topEmission;
        if (_danger) top += _palette.DangerFace;
        _overlay.SetColor(FaceEmissionIds[(int)CubeFace.Top], top);
        _overlay.SetColor(FaceEmissionIds[(int)CubeFace.Side], _terrain != null ? _terrain.SideEmission : _sideEmission);
    }

    private void Apply()
    {
        _overlay.SetColor(OutlineColorId, _edgeColor);
        _overlay.SetColor(EdgeEmissionId, _edgeEmission);
        _overlay.SetColor(FaceEmissionIds[(int)CubeFace.Bottom], _bottomEmission);
        ApplyFaces();
    }
}
