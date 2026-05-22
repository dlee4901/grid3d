using System;

// Pure-C# logging facade for Grid2D backend.
// Default: no-op. The Unity layer wires Log.Info/Warning/Error to Debug.Log/LogWarning/LogError at startup.
public static class GridLog
{
    public static Action<string> Info    { get; set; } = _ => { };
    public static Action<string> Warning { get; set; } = _ => { };
    public static Action<string> Error   { get; set; } = _ => { };
}
