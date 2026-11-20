using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InteractableUI : LoggableBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public event Action OnClickCompleted;
    public event Action OnHoldTriggered;
    public event Action OnHoldEnded;

    [Header("Hold Time")]
    [FormerlySerializedAs("_triggerTime")]
    [FormerlySerializedAs("_hoverTriggerTime")]
    [SerializeField] private float _holdTime = 0.5f;

    [Header("Overlay Colors")]
    [SerializeField] private Color32 _defaultOverlayColor = new Color32(255, 255, 255, 0);
    [SerializeField] private Color32 _hoverOverlayColor = new Color32(255, 255, 255, 5);
    [SerializeField] private Color32 _pressOverlayColor = new Color32(0, 0, 0, 10);

    [Header("Visual Scales")]
    [SerializeField] private float _hoverScale = 1.02f;
    [SerializeField] private float _pressScale = 0.98f;
    [SerializeField] private List<GameObject> _scaleTargets;

    private Image _overlayImage;
    private Vector3 _baseScale = Vector3.one;

    private bool _inside;
    private bool _pressed;
    private bool _held;
    private float _pressedFor;

    private void Start()
    {
        var child = new GameObject("Overlay").AddComponent<Image>();
        child.rectTransform.anchorMin = new Vector2(0, 0);
        child.rectTransform.anchorMax = new Vector2(1, 1);
        child.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
        child.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        child.transform.SetParent(transform, false);
        _overlayImage = child.GetComponent<Image>();
        _baseScale = transform.localScale;
        ApplyVisual();
    }

    private void Update()
    {
        if (!_pressed || _held) return;
        _pressedFor += Time.deltaTime;
        if (_pressedFor < _holdTime) return;
        _held = true;
        OnHoldTriggered?.Invoke();
    }

    private void OnDisable()
    {
        _inside = false;
        CancelPress();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _inside = true;
        ApplyVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _inside = false;
        CancelPress();
        ApplyVisual();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        _held = false;
        _pressedFor = 0f;
        ApplyVisual();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_pressed) return;
        var held = _held;
        CancelPress();
        ApplyVisual();
        if (!held) OnClickCompleted?.Invoke();
    }

    private void CancelPress()
    {
        _pressed = false;
        if (!_held) return;
        _held = false;
        OnHoldEnded?.Invoke();
    }

    private void ApplyVisual()
    {
        if (_overlayImage == null) return;
        _overlayImage.color = _pressed ? _pressOverlayColor : _inside ? _hoverOverlayColor : _defaultOverlayColor;
        ChangeScale(_pressed ? _pressScale : _inside ? _hoverScale : 1f);
    }

    private void ChangeScale(float scale)
    {
        if (_scaleTargets == null || _scaleTargets.Count == 0)
        {
            transform.localScale = _baseScale * scale;
            return;
        }
        foreach (var target in _scaleTargets) target.transform.localScale = _baseScale * scale;
    }
}
