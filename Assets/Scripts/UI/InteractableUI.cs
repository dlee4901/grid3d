using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public event Action OnHoverTriggered;
    public event Action OnHoverEnded;
    
    public event Action OnHoldTriggered;
    public event Action OnHoldEnded;
    
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
                Debug.Log("hover trigger for " + _hoverTriggerTime + " seconds");
                StopHoverEvent();
                OnHoverTriggered?.Invoke();
            }
        }
        if (_isHeld)
        {
            _holdDuration += Time.deltaTime;
            if (_holdDuration >= _holdTriggerTime)
            {
                Debug.Log("hold trigger for " + _holdTriggerTime + " seconds");
                StopHoldEvent(true);
                OnHoldTriggered?.Invoke();
            }
        }
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
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _overlayImage.color = _hoverOverlayColor;
        ChangeScale(_hoverScale);
        StartHoverEvent();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _overlayImage.color = _defaultOverlayColor;
        ChangeScale();
        StopHoverEvent();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _overlayImage.color = _pressOverlayColor;
        ChangeScale(_pressScale);
        //StopHoverEvent();
        StartHoldEvent();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        _overlayImage.color = _defaultOverlayColor;
        ChangeScale();
        StopHoldEvent();
    }
    
    public void StartHoverEvent()
    {
        _hoverDuration = 0f;
        _isHovering = true;
    }
    
    public void StopHoverEvent()
    {
        _hoverDuration = 0f;
        _isHovering = false;
        OnHoverEnded?.Invoke();
    }
    
    public void StartHoldEvent()
    {
        _holdDuration = 0f;
        _isHeld = true;
    }
    
    public void StopHoldEvent(bool noClick=false)
    {
        if (!noClick && _holdDuration > 0f)
        {
            Debug.Log("click trigger");
            OnClickCompleted?.Invoke();
        }
        _holdDuration = 0f;
        _isHeld = false;
        OnHoldEnded?.Invoke();
    }
}