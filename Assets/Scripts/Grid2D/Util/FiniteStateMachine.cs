using System;
using System.Collections.Generic;

public class FiniteStateMachine<TState> where TState : struct, Enum
{
    private readonly Dictionary<TState, Action> _onEnter = new();
    private readonly Dictionary<TState, Action> _onExit = new();
    private readonly Dictionary<TState, Action> _onUpdate = new();

    public TState Current { get; private set; }
    public TState? Previous { get; private set; }

    public event Action<TState, TState> OnTransition;

    public FiniteStateMachine(TState initial)
    {
        Current = initial;
    }

    public FiniteStateMachine<TState> OnEnter(TState state, Action handler)  { _onEnter[state]  = handler; return this; }
    public FiniteStateMachine<TState> OnExit(TState state, Action handler)   { _onExit[state]   = handler; return this; }
    public FiniteStateMachine<TState> OnUpdate(TState state, Action handler) { _onUpdate[state] = handler; return this; }

    public void TransitionTo(TState next)
    {
        if (Current.Equals(next)) return;
        if (_onExit.TryGetValue(Current, out var exit)) exit?.Invoke();
        Previous = Current;
        Current = next;
        if (_onEnter.TryGetValue(next, out var enter)) enter?.Invoke();
        OnTransition?.Invoke(Previous.Value, next);
    }

    public void Tick()
    {
        if (_onUpdate.TryGetValue(Current, out var update)) update?.Invoke();
    }

    public bool Is(TState state) => Current.Equals(state);
}
