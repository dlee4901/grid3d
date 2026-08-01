using System.Collections.Generic;

public interface IGridRenderer
{
    void ClearHighlights();
    void HighlightPositions(GridSteps steps, GridHighlightType type);
}
