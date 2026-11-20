using TMPro;
using UnityEngine;

public class PlayerTimerView : LoggableBehaviour
{
    [SerializeField] private TMP_Text _playerText;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private Color _activeColor = Color.white;
    [SerializeField] private Color _inactiveColor = new Color(1f, 1f, 1f, 0.4f);

    private int _shownSeconds = int.MinValue;
    private Color _teamColor = Color.white;

    public void SetLabel(string label) => _playerText.text = label;

    public void SetTeamColor(Color color) => _teamColor = color;

    public void SetTimeMs(int ms)
    {
        var seconds = ms / 1000;
        if (seconds == _shownSeconds) return;
        _shownSeconds = seconds;
        _timeText.text = TimerClock.Format(ms);
    }

    public void SetActiveVisual(bool active)
    {
        var color = active ? _activeColor : _inactiveColor;
        _playerText.color = new Color(_teamColor.r, _teamColor.g, _teamColor.b, color.a);
        _timeText.color = color;
    }
}
