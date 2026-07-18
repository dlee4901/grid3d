using System;
using TMPro;
using UnityEngine;

public class PlayerTimer : LoggableBehaviour
{
    [SerializeField] private TMP_Text _playerText;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private Color _activeColor = Color.white;
    [SerializeField] private Color _inactiveColor = new Color(1f, 1f, 1f, 0.4f);

    private int _bankMs;
    private int _remainingMs;
    private float _accum;
    private bool _running;

    public bool IsRunning => _running;
    public int ElapsedMs => Mathf.Max(0, Mathf.RoundToInt(_accum * 1000f));

    public event Action Expired;

    private void Update()
    {
        if (!_running) return;
        _accum += Time.deltaTime;
        var remaining = Mathf.Max(0, _bankMs - Mathf.RoundToInt(_accum * 1000f));
        if (remaining != _remainingMs)
        {
            _remainingMs = remaining;
            UpdateTimeText(_remainingMs);
        }
        if (remaining <= 0)
        {
            _running = false;
            Expired?.Invoke();
        }
    }

    public void SetLabel(string label) => _playerText.text = label;

    public void SetActiveVisual(bool active)
    {
        var color = active ? _activeColor : _inactiveColor;
        _playerText.color = color;
        _timeText.color = color;
    }

    public void SetTimeMs(int ms)
    {
        _remainingMs = Mathf.Max(0, ms);
        _bankMs = _remainingMs;
        _accum = 0f;
        UpdateTimeText(_remainingMs);
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

    private void UpdateTimeText(int ms)
    {
        var totalSeconds = ms / 1000;
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        _timeText.text = $"{minutes:00}:{seconds:00}";
    }
}
