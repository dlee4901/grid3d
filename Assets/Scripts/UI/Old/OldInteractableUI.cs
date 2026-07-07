using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OldInteractableUI : LoggableBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public event Action OnHoverTriggered;
    public event Action OnHoverCompleted;
    
    public event Action OnHoldTriggered;
    public event Action OnHoldCompleted;
    
    public event Action OnClickCompleted;
    
    [Header("Trigger Times")]
    [SerializeField] private float _hoverTriggerTime = 0.1f;
    [SerializeField] private float _holdTriggerTime = 1f;
    
    [Header("Overlay Colors")]
    [SerializeField] private Color32 _defaultOverlayColor = new Color32(255, 255, 255, 0);
    [SerializeField] private Color32 _hoverOverlayColor = new Color32(255, 255, 255, 5);
    [SerializeField] private Color32 _pressOverlayColor = new Color32(0, 0, 0, 10);
    
    [Header("Visual Scales")]
    [SerializeField] private float _hoverScale = 1.02f;
    [SerializeField] private float _pressScale = 0.98f;
    [SerializeField] private List<GameObject> _scaleTargets;

    private float _hoverDuration;
    private float _holdDuration;
    private bool _isHovering;
    private bool _isHeld;
    
    private Image _overlayImage;
    private Vector3 _baseScale = Vector3.one;
    
    private void Start()
    {
        var child = new GameObject("Overlay").AddComponent<Image>();
        child.rectTransform.anchorMin = new Vector2(0, 0);
        child.rectTransform.anchorMax = new Vector2(1, 1);
        child.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
        child.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
        child.transform.SetParent(transform, false);
        _overlayImage = child.GetComponent<Image>();
        _overlayImage.color = _defaultOverlayColor;
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        if (_isHovering)
        {
            _hoverDuration += Time.deltaTime;
            if (_hoverDuration >= _hoverTriggerTime)
            {
                Log($"HoverTriggered ({_hoverTriggerTime}s)");
                CompleteHoverEvent();
                OnHoverTriggered?.Invoke();
            }
        }
        if (_isHeld)
        {
            _holdDuration += Time.deltaTime;
            if (_holdDuration >= _holdTriggerTime)
            {
                Log($"HoldTriggered ({_holdTriggerTime}s)");
                CompleteHoldEvent(true);
                OnHoldTriggered?.Invoke();
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Log("PointerEnter");
        _overlayImage.color = _hoverOverlayColor;
        ChangeScale(_hoverScale);
        StartHoverEvent();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Log("PointerExit");
        _overlayImage.color = _defaultOverlayColor;
        ChangeScale();
        CompleteHoverEvent();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Log("PointerDown");
        _overlayImage.color = _pressOverlayColor;
        ChangeScale(_pressScale);
        //StopHoverEvent();
        StartHoldEvent();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Log("PointerUp");
        _overlayImage.color = _defaultOverlayColor;
        ChangeScale();
        CompleteHoldEvent();
    }
    
    private void StartHoverEvent()
    {
        BeginHoverTimer();
    }
    
    private void CompleteHoverEvent()
    {
        EndHoverTimer();
        Log("HoverEnded");
        OnHoverCompleted?.Invoke();
    }
    
    private void StartHoldEvent()
    {
        BeginHoldTimer();
        CompleteHoverEvent();
    }

    private void CompleteHoldEvent(bool noClick=false)
    {
        if (!noClick && _holdDuration > 0f)
        {
            Log("ClickCompleted");
            OnClickCompleted?.Invoke();
        }
        EndHoldTimer();
        Log("HoldEnded");
        OnHoldCompleted?.Invoke();
    }
    
    private void BeginHoverTimer()
    {
        _hoverDuration = 0f;
        _isHovering = true;
    }
    
    private void EndHoverTimer()
    {
        _hoverDuration = 0f;
        _isHovering = false;
    }
    
    private void BeginHoldTimer()
    {
        _holdDuration = 0f;
        _isHeld = true;
    }
    
    private void EndHoldTimer()
    {
        _holdDuration = 0f;
        _isHeld = false;
    }
    
    private void ChangeScale(float scale=1.0f)
    {
        if (_scaleTargets == null || _scaleTargets.Count == 0)
        {
            transform.localScale = _baseScale * scale;
        }
        else
        {
            foreach (var target in _scaleTargets)
            {
                target.transform.localScale = _baseScale * scale;
            }
        }
    }
}