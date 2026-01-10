using System.Collections.Generic;

public abstract class GameAction
{
    public abstract IEnumerable<Effect> Resolve(Grid2D grid);
}
