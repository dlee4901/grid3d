using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LinePool
{
    private readonly Transform _parent;
    private readonly Material _material;
    private readonly List<LineRenderer> _lines = new();
    private int _shown;

    public LinePool(Transform parent, Material material)
    {
        _parent = parent;
        _material = material;
    }

    public void HideAll()
    {
        foreach (var line in _lines) line.gameObject.SetActive(false);
        _shown = 0;
    }

    public void Show(List<Vector3> points, float width)
    {
        var line = Next();
        line.widthMultiplier = width;
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
        line.gameObject.SetActive(true);
    }

    private LineRenderer Next()
    {
        if (_shown < _lines.Count) return _lines[_shown++];
        var line = new GameObject("Line").AddComponent<LineRenderer>();
        line.transform.SetParent(_parent);
        line.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        line.alignment = LineAlignment.TransformZ;
        line.sharedMaterial = _material;
        line.numCornerVertices = 4;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        _lines.Add(line);
        _shown++;
        return line;
    }
}
