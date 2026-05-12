using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityHFSM;

public class HoldClickable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float _holdDuration;

    [SerializeField] private UnityEvent OnPressed;
    [SerializeField] private UnityEvent OnReleased;
    [SerializeField] private UnityEvent OnClicked;
    [SerializeField] private UnityEvent OnHeld;

    private float _elapsedTime;
    private StateMachine _stateMachine;

    void Start()
    {
        InitStateMachine();
    }

    void Update()
    {
        _stateMachine.OnLogic();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPressed?.Invoke();
        _stateMachine.Trigger("OnPointerDown");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnReleased?.Invoke();
        if (_stateMachine.ActiveState.name == "Pressed") OnClicked.Invoke();
        _stateMachine.Trigger("OnPointerUp");
    }

    public void InitStateMachine()
    {
        _stateMachine = new StateMachine();

        _stateMachine.AddState("Unpressed");
        _stateMachine.AddState("Pressed", onEnter: state => OnEnterPressedState(), onLogic: state => OnLogicPressedState());
        _stateMachine.AddState("Held", onEnter: state => OnEnterHeldState());

        _stateMachine.AddTriggerTransition("OnPointerDown", "Unpressed", "Pressed");
        _stateMachine.AddTriggerTransition("OnPointerUp", "Pressed", "Unpressed");
        _stateMachine.AddTriggerTransition("OnPointerUp", "Held", "Unpressed");
        _stateMachine.AddTriggerTransition("OnPointerHeld", "Pressed", "Held");

        _stateMachine.SetStartState("Unpressed");
        _stateMachine.Init();
    }

    public void OnEnterPressedState()
    {
        _elapsedTime = 0f;
    }

    public void OnLogicPressedState()
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime > _holdDuration)
        {
            _stateMachine.Trigger("OnPointerHeld");
        }
    }

    public void OnEnterHeldState()
    {
        OnHeld?.Invoke();
    } 
}