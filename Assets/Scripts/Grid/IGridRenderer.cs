using System.Collections.Generic;
using UnityEngine;

public interface IGridRenderer
{
    void ClearHighlights();
    void HighlightPositions(HashSet<GridPosition> positions, GridHighlightType type);
    void HighlightPositions(GridSteps steps, GridHighlightType type);
    Color HighlightColor(GridHighlightType type);
}
