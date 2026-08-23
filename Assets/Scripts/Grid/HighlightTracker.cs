using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class HighlightTracker : IGridRenderer
{
    private readonly IGridRenderer _inner;
    private readonly Dictionary<int, GridHighlightType> _highlights = new();

    public event Action Changed;

    public HighlightTracker(IGridRenderer inner) => _inner = inner;

    public bool TryGet(GridPosition position, out GridHighlightType type) => _highlights.TryGetValue(position.Dim1, out type);

    public Color HighlightColor(GridHighlightType type) => _inner.HighlightColor(type);
  
    public void ClearHighlights()
    {
        _inner.ClearHighlights();
        _highlights.Clear();
        Changed?.Invoke();
    }

    public void HighlightPositions(HashSet<GridPosition> positions,
        GridHighlightType type)
    {
        _inner.HighlightPositions(positions, type);
        foreach (var position in positions) _highlights[position.Dim1] = type;
        Changed?.Invoke();
    }

    public void HighlightPositions(GridSteps steps, GridHighlightType type)
    {
        _inner.HighlightPositions(steps, type);
        for (var group = 0; group < steps.GroupCount; group++)
            foreach (var position in steps.GetPositions(group))
                _highlights[position.Dim1] = type;
        Changed?.Invoke();
    }
}