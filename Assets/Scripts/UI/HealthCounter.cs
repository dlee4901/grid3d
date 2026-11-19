using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthCounter : LoggableBehaviour
{
    [SerializeField] private Image _healthIcon;
    [SerializeField] private TMP_Text _counterText;
    [SerializeField] private Sprite _deathSprite;
    
    [SerializeField] private float _size = 48f;
    [SerializeField] private float _fontScale = 0.5f;
    [SerializeField] private TeamPalette _teamPalette;
    [SerializeField] private Image _border;
    [SerializeField] private int _borderAlpha = 64;

    private const float BorderSpriteMargin = 2f / 56f;

    private int _current = int.MinValue;
    private Transform _counterTextTransform;
    private Sprite _aliveSprite;
    private Color _ownerColor = Color.white;
    private Color? _textColor;
    
    public bool ForceVisible { get; set; }
    public int Change { get; private set; }
    public bool Dead { get; private set; }

    private void Start()
    {
        _healthIcon.raycastTarget = false;
        _counterText.raycastTarget = false;
        SetSize(_size);
    }
    
    // 18 48
    // 8 24
    
    public void SetSize(float size)
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
        _border.rectTransform.sizeDelta = Vector2.one * (size * 2f * BorderSpriteMargin);
        _counterText.fontSize = size * _fontScale;
    }

    public void SetHealthCount(int health)
    {
        if (health == _current) return;
        _current = health;
        var healthText = health.ToString();
        _counterText.rectTransform.offsetMin = new Vector2(_counterText.rectTransform.offsetMin.x, healthText.Length > 1 ? _size/6f : _size/12f);
        _counterText.text = health.ToString();
    }
    
    public void SetPrevious(int previous)
    {
        Change = _current - previous;
    }

    public void SetPulse(float amount)
    {
        _textColor ??= _counterText.color;
        var color = _textColor.Value;
        color.a *= 1f - amount;
        _counterText.color = color;
    }

    public void SetDead(bool dead)
    {
        if (dead == Dead) return;
        Dead = dead;
        if (_aliveSprite == null) _aliveSprite = _healthIcon.sprite;
        _healthIcon.sprite = dead ? _deathSprite : _aliveSprite;
        _counterText.gameObject.SetActive(!dead);
        _border.gameObject.SetActive(!dead);
        ApplyIconColor();
    }

    public void SetOwner(Entity entity, int viewerPlayer)
    {
        _ownerColor = _teamPalette.ColorFor(entity, viewerPlayer);
        ApplyIconColor();
    }

    private void ApplyIconColor()
    {
        var color = Dead ? Color.white : _ownerColor;
        color.a = _healthIcon.color.a;
        _healthIcon.color = color;
        ApplyBorderColor();
    }

    public void SetAlpha(float alpha)
    {
        var color = _healthIcon.color;
        color.a = alpha;
        _healthIcon.color = color;
        ApplyBorderColor();
    }

    private void ApplyBorderColor()
    {
        var color = _ownerColor;
        color.a = _borderAlpha / 255f * _healthIcon.color.a;
        _border.color = color;
    }
}