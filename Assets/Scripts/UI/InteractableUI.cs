using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float _hoverTriggerTime;
    [SerializeField] private float _holdTriggerTime;
    
    private float _hoverDuration;
    private float _holdDuration;
    private bool _isHovering;
    private bool _isHeld;
    
    private bool _hoverEvent;
    private bool _holdEvent;
    
    void Start()
    {
        _hoverDuration = 0f;
        _holdDuration = 0f;
        _hoverEvent = false;
        _holdEvent = false;
    }
    
    void Update()
    {
        if (_isHovering)
        {
            _hoverDuration += Time.deltaTime;
            if (!_hoverEvent && _hoverDuration >= _hoverTriggerTime)
            {
                Debug.Log("hover trigger for " + _hoverTriggerTime + " seconds");
                _hoverEvent = true;
            }
        }
        if (_isHeld)
        {
            _holdDuration += Time.deltaTime;
            if (!_holdEvent && _holdDuration >= _holdTriggerTime)
            {
                Debug.Log("hold trigger for " + _holdTriggerTime + " seconds");
                _holdEvent = true;
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoverDuration = 0f;
        _isHovering = true;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        _hoverDuration = 0f;
        _isHovering = false;
        _hoverEvent = false;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _holdDuration = 0f;
        _isHeld = true;
        
        _hoverDuration = 0f;
        _isHovering = false;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        _holdDuration = 0f;
        _isHeld = false;
        _holdEvent = false;
    }
}