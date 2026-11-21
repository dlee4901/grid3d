using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragRelay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public event Action<PointerEventData> Began;
    public event Action<PointerEventData> Moved;
    public event Action<PointerEventData> Ended;

    public void OnBeginDrag(PointerEventData eventData) => Began?.Invoke(eventData);
    public void OnDrag(PointerEventData eventData) => Moved?.Invoke(eventData);
    public void OnEndDrag(PointerEventData eventData) => Ended?.Invoke(eventData);
}
