using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractableUI : LoggableBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
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
    
    private Image _overlayImage;
    private Vector3 _baseScale = Vector3.one;
    
    private float _hoverTriggerDuration;
    private float _holdTriggerDuration;
    
    private enum MouseState { None, Hover, HoverTriggered, Hold, HoldTriggered }
    private FiniteStateMachine<MouseState> _fsm;
    
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
        
        InitFsm();
    }

    private void Update()
    {
        _fsm.Tick();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Log("PointerEnter");
        _fsm.TransitionTo(MouseState.Hover);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Log("PointerExit");
        _fsm.TransitionTo(MouseState.None);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Log("PointerDown");
        _fsm.TransitionTo(MouseState.Hold);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Log("PointerUp");
        if (_fsm.Is(MouseState.Hold))
        {
            OnClickCompleted?.Invoke();
        }
        _fsm.TransitionTo(MouseState.None);
    }
    
    private void InitFsm()
    {
        _fsm = new FiniteStateMachine<MouseState>(MouseState.None);
        _fsm.OnEnter(MouseState.None, () =>
        {
            _overlayImage.color = _defaultOverlayColor;
            ChangeScale();
            _hoverTriggerDuration = 0;
            _holdTriggerDuration = 0;
        });
        _fsm.OnEnter(MouseState.Hover, () =>
        {
            _overlayImage.color = _hoverOverlayColor;
            ChangeScale(_hoverScale);
            _hoverTriggerDuration = 0;
        });
        _fsm.OnUpdate(MouseState.Hover, () =>
        {
            _hoverTriggerDuration += Time.deltaTime;
            if (_hoverTriggerDuration > _hoverTriggerTime)
            {
                _fsm.TransitionTo(MouseState.HoverTriggered);
            }
        });
        _fsm.OnExit(MouseState.Hover, () =>
        {
            _hoverTriggerDuration = 0;
        });
        _fsm.OnEnter(MouseState.HoverTriggered, () =>
        {
            OnHoverTriggered?.Invoke();
        });
        _fsm.OnExit(MouseState.HoverTriggered, () =>
        {
            OnHoverCompleted?.Invoke();
        });
        _fsm.OnEnter(MouseState.Hold, () =>
        {
            _overlayImage.color = _pressOverlayColor;
            ChangeScale(_pressScale);
            _holdTriggerDuration = 0;
        });
        _fsm.OnUpdate(MouseState.Hold, () =>
        {
            _holdTriggerDuration += Time.deltaTime;
            if (_holdTriggerDuration > _holdTriggerTime)
            {
                _fsm.TransitionTo(MouseState.HoldTriggered);
            }
        });
        _fsm.OnExit(MouseState.Hold, () =>
        {
            _holdTriggerDuration = 0;
        });
        _fsm.OnEnter(MouseState.HoldTriggered, () =>
        {
            OnHoldTriggered?.Invoke();
        });
        _fsm.OnExit(MouseState.HoldTriggered, () =>
        {
            OnHoldCompleted?.Invoke();
        });
    }
    
    // private void StartHoverEvent()
    // {
    //     BeginHoverTimer();
    // }
    //
    // private void CompleteHoverEvent()
    // {
    //     EndHoverTimer();
    //     Log("HoverEnded");
    //     OnHoverCompleted?.Invoke();
    // }
    //
    // private void StartHoldEvent()
    // {
    //     BeginHoldTimer();
    //     CompleteHoverEvent();
    // }
    //
    // private void CompleteHoldEvent(bool noClick=false)
    // {
    //     if (!noClick && _holdTriggerDuration > 0f)
    //     {
    //         Log("ClickCompleted");
    //         OnClickCompleted?.Invoke();
    //     }
    //     EndHoldTimer();
    //     Log("HoldEnded");
    //     OnHoldCompleted?.Invoke();
    // }
    //
    // private void BeginHoverTimer()
    // {
    //     _hoverTriggerDuration = 0f;
    //     _isHovering = true;
    // }
    //
    // private void EndHoverTimer()
    // {
    //     _hoverTriggerDuration = 0f;
    //     _isHovering = false;
    // }
    //
    // private void BeginHoldTimer()
    // {
    //     _holdTriggerDuration = 0f;
    //     _isHeld = true;
    // }
    //
    // private void EndHoldTimer()
    // {
    //     _holdTriggerDuration = 0f;
    //     _isHeld = false;
    // }
    
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