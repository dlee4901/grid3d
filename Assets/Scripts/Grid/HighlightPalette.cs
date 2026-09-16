using System;
using UnityEngine;

[Serializable]
public struct HighlightStyle
{
    public Color Color;
    [Min(0f)] public float EdgeIntensity;
    [Range(0f, 1f)] public float UIAlpha;
}

[CreateAssetMenu(fileName = "HighlightPalette", menuName = "Grid3D/Highlight Palette")]
public class HighlightPalette : ScriptableObject
{
    [SerializeField] private HighlightStyle _availableSources;
    [SerializeField] private HighlightStyle _availableTargets;
    [SerializeField] private HighlightStyle _selectionArea;
    [SerializeField] private HighlightStyle _effectArea;

    public HighlightStyle StyleFor(HighlightType type) => type switch
    {
        HighlightType.AvailableSources => _availableSources,
        HighlightType.AvailableTargets => _availableTargets,
        HighlightType.SelectionArea    => _selectionArea,
        HighlightType.EffectArea       => _effectArea,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    public Color EdgeEmissionFor(HighlightType type)
    {
        var style = StyleFor(type);
        return new Color(style.Color.r * style.EdgeIntensity,
                         style.Color.g * style.EdgeIntensity,
                         style.Color.b * style.EdgeIntensity, 1f);
    }

    public Color UIColorFor(HighlightType type)
    {
        var style = StyleFor(type);
        var color = style.Color;
        color.a = style.UIAlpha;
        return color;
    }
}
