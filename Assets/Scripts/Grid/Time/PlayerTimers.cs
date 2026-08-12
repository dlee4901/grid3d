using System;

public class PlayerTimers
{
    private TimerClock[] _clocks;
    private int _players;

    public event Action<int> SeatExpired;

    public void Init(int players)
    {
        _clocks = null;
        _players = 0;
        if (players < 1) return;

        _players = players;
        _clocks = new TimerClock[players + 1];
        for (var seat = 1; seat <= players; seat++) _clocks[seat] = new TimerClock();
    }

    public void Tick(float deltaTime)
    {
        for (var seat = 1; seat <= _players; seat++)
        {
            var clock = _clocks[seat];
            if (!clock.IsRunning) continue;
            if (clock.Tick(deltaTime)) SeatExpired?.Invoke(seat);
        }
    }

    public void SetTimeMs(int seat, int ms)
    {
        if (IsValidSeat(seat)) _clocks[seat].SetTimeMs(ms);
    }

    public void SetRunning(int seat, bool running)
    {
        if (IsValidSeat(seat)) _clocks[seat].SetRunning(running);
    }

    public bool IsRunning(int seat) => IsValidSeat(seat) && _clocks[seat].IsRunning;

    public int RemainingMs(int seat) => IsValidSeat(seat) ? _clocks[seat].RemainingMs : 0;

    public int ElapsedMs(int seat) => IsValidSeat(seat) ? _clocks[seat].ElapsedMs : 0;

    private bool IsValidSeat(int seat) => _clocks != null && seat >= 1 && seat <= _players;
}
