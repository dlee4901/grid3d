using UnityEngine;

public abstract class LoggableBehaviour : MonoBehaviour
{
    [SerializeField] protected bool _debug;

    protected void Log(string message)
    {
        if (!_debug) return;
        Debug.Log($"[{GetType().Name}:{name}] {message}");
    }

    protected void LogWarning(string message)
    {
        if (!_debug) return;
        Debug.LogWarning($"[{GetType().Name}:{name}] {message}");
    }

    protected void LogError(string message)
    {
        Debug.LogError($"[{GetType().Name}:{name}] {message}");
    }

    protected void LogException(System.Exception ex)
    {
        Debug.LogException(ex, this);
    }
}
