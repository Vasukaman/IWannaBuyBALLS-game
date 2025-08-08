// Filename: IntGameEvent.cs
using Core.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// A ScriptableObject that acts as a global event channel for events
    /// that need to pass an integer value as a parameter.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBallGameEvent", menuName = "Game Events/Ball Game Event")]
    public class BallGameEvent : ScriptableObject
    {
        private readonly List<Action<IBallView>> _listeners = new List<Action<IBallView>>();

        public void Raise(IBallView value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke(value);
            }
        }

        public void RegisterListener(Action<IBallView> listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(Action<IBallView> listener)
        {
            if (_listeners.Contains(listener))
            {
                _listeners.Remove(listener);
            }
        }
    }
}