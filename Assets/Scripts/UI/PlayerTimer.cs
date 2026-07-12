using System;
using TMPro;
using UnityEngine;

/// Per-player timer display. Ticks locally (a prediction of the authoritative GridState bank) while
/// running, snaps to the bank via SetTimeMs on state changes, and raises Expired when it hits zero.
/// The countdown is wall-clock (frontend only) — the authoritative bank lives in GridState.
public class PlayerTimer : LoggableBehaviour
{
    [SerializeField] private TMP_Text _playerText;
    [SerializeField] private TMP_Text _timeText;

    private int _bankMs;         // authoritative time when the current run started
    private int _remainingMs;    // currently displayed remaining time
    private float _accum;        // seconds elapsed in the current run
    private bool _running;

    public bool IsRunning => _running;
    public int ElapsedMs => Mathf.Max(0, Mathf.RoundToInt(_accum * 1000f));

    /// Fires once when a running timer reaches zero.
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

    /// Snap the display to an authoritative value (from GridState) and reset the local run.
    public void SetTimeMs(int ms)
    {
        _remainingMs = Mathf.Max(0, ms);
        _bankMs = _remainingMs;
        _accum = 0f;
        UpdateTimeText(_remainingMs);
    }

    public void SetRunning(bool running)
    {
        if (running && !_running)   // (re)start the local run from the current displayed value
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
