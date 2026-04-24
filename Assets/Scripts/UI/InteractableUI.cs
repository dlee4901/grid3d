using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public event Action OnHoverComplete;
    public event Action OnHoldComplete;
    public event Action OnClickComplete;
    
    [SerializeField] private float _hoverTriggerTime;
    [SerializeField] private float _holdTriggerTime;
    
    private float _hoverDuration;
    private float _holdDuration;
    private bool _isHovering;
    private bool _isHeld;
    
    private Image _overlayImage;
    private readonly Color _defaultColor = new Color(1f, 1f, 1f, 0f);
    private readonly Color _hoverColor = new Color(1f, 1f, 1f, 0.05f);
    private readonly Color _pressColor = new Color(0f, 0f, 0f, 0.20f);
    private Vector3 _baseScale = Vector3.one;
    private readonly float _hoverScale = 1.03f;
    private readonly float _pressScale = 0.97f;

    private void Start()
    {
        var child = new GameObject("Overlay").AddComponent<Image>();
        child.transform.SetParent(transform, false);
        _overlayImage = child.GetComponent<Image>();
        _overlayImage.color = _defaultColor;
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
                OnHoverComplete?.Invoke();
            }
        }
        if (_isHeld)
        {
            _holdDuration += Time.deltaTime;
            if (_holdDuration >= _holdTriggerTime)
            {
                Debug.Log("hold trigger for " + _holdTriggerTime + " seconds");
                StopHoldEvent(true);
                OnHoldComplete?.Invoke();
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _overlayImage.color = _hoverColor;
        transform.localScale = _baseScale * _hoverScale;
        StartHoverEvent();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _overlayImage.color = _defaultColor;
        transform.localScale = _baseScale;
        StopHoverEvent();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _overlayImage.color = _pressColor;
        transform.localScale = _baseScale * _pressScale;
        StopHoverEvent();
        StartHoldEvent();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        _overlayImage.color = _defaultColor;
        transform.localScale = _baseScale;
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
            OnClickComplete?.Invoke();
        }
        _holdDuration = 0f;
        _isHeld = false;
    }
}