public class TraversalConfig
{
    public DirectionType Direction { get; set; }

    public int MaxDistance { get; set; } = 0; // -1: inf
    public bool AbsoluteDirection { get; set; } = false;
    public bool Linear { get; set; } = false;
    public bool Trace { get; set; } = false;

    public int StartWidth { get; set; } = 0;
    public int DeltaWidth { get; set; } = 0;
    public int DeltaWidthStep { get; set; } = 1;
    public int DeltaWidthDistanceOffset { get; set; } = 0;

    public EntityPassthrough Passthrough { get; set; } = EntityPassthrough.None;
    public PredicateConfig? PassthroughQuery { get; set; }

    public TraversalConfig? Chain { get; set; }
    public int ChainOffset { get; set; } = 0; // IF (n > 0) n ~ distance ELSE maxDistReached + n ~ maxDistReached
}
