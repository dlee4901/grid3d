using UnityEngine;

public class TimerClock
{
    private int _bankMs;
    private int _remainingMs;
    private float _accum;
    private bool _running;

    public bool IsRunning => _running;
    public int RemainingMs => _remainingMs;
    public int ElapsedMs => Mathf.Max(0, Mathf.RoundToInt(_accum * 1000f));

    public void SetTimeMs(int ms)
    {
        _remainingMs = Mathf.Max(0, ms);
        _bankMs = _remainingMs;
        _accum = 0f;
    }

    public void SetRunning(bool running)
    {
        if (running && !_running)
        {
            _bankMs = _remainingMs;
            _accum = 0f;
        }
        _running = running;
    }

    public bool Tick(float deltaTime)
    {
        if (!_running) return false;
        _accum += deltaTime;
        _remainingMs = Mathf.Max(0, _bankMs - Mathf.RoundToInt(_accum * 1000f));
        if (_remainingMs > 0) return false;
        _running = false;
        return true;
    }

    public static string Format(int ms)
    {
        var totalSeconds = ms / 1000;
        return $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }
}
