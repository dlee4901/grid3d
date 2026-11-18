using System.Collections.Generic;
using UnityEngine;

public class MovePathRenderer : LoggableBehaviour
{
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private GridRenderer _gridRenderer;
    [SerializeField] private Material _arrowMaterial;
    [SerializeField] private float _arrowWidth = 0.12f;

    private LinePool _arrows;

    private const float ArrowHeight = 0.05f;

    public void Build()
    {
        _arrows = new LinePool(transform, _arrowMaterial);
        _gridManager.DisplayChanged += Show;
    }

    private void OnDestroy()
    {
        if (_gridManager != null) _gridManager.DisplayChanged -= Show;
    }

    private void Show()
    {
        _arrows.HideAll();
        if (!_gridManager.IsPreviewing) return;
        var paths = new List<List<Vector3>>();
        var pathIndexByUid = new Dictionary<int, int>();
        foreach (var move in _gridManager.DisplayedState.Moves)
        {
            if (!pathIndexByUid.TryGetValue(move.Uid, out var index))
            {
                index = paths.Count;
                pathIndexByUid[move.Uid] = index;
                paths.Add(new List<Vector3> { _gridRenderer.CellCenter(move.From, ArrowHeight) });
            }
            paths[index].Add(_gridRenderer.CellCenter(move.To, ArrowHeight));
        }
        foreach (var path in paths) _arrows.Show(path, _arrowWidth);
    }
}
