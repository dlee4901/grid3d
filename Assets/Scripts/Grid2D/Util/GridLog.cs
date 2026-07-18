using System;

public static class GridLog
{
    public static Action<string> Info    { get; set; } = _ => { };
    public static Action<string> Warning { get; set; } = _ => { };
    public static Action<string> Error   { get; set; } = _ => { };
}
