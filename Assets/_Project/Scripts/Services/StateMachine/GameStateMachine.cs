// Filename: GameStateMachine.cs
// Location: _Project/Scripts/Services/StateMachine/
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateMachine
{
    private readonly Dictionary<Type, IState> _states;
    private IState _currentState;

    public GameStateMachine(Dictionary<Type, IState> states)
    {
        _states = states;
    }

    public void Enter<TState>() where TState : IState
    {
        if (!_states.TryGetValue(typeof(TState), out var newState))
        {
            Debug.LogError($"State of type {typeof(TState)} not found!");
            return;
        }

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Tick()
    {
        _currentState?.Tick();
    }
}