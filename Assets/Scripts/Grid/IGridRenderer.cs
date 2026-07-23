using System.Collections.Generic;

public interface IGridRenderer
{
    void ClearHighlights();
    void HighlightPositions(IEnumerable<(int, int)> positions, GridHighlightType type);
    void HighlightPositions(IEnumerable<int> positions, GridHighlightType type);
}
