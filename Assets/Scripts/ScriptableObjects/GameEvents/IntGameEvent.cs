// Filename: IntGameEvent.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// A ScriptableObject that acts as a global event channel for events
    /// that need to pass an integer value as a parameter.
    /// </summary>
    [CreateAssetMenu(fileName = "NewIntGameEvent", menuName = "Game Events/Int Game Event")]
    public class IntGameEvent : ScriptableObject
    {
        private readonly List<Action<int>> _listeners = new List<Action<int>>();

        public void Raise(int value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke(value);
            }
        }

        public void RegisterListener(Action<int> listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(Action<int> listener)
        {
            if (_listeners.Contains(listener))
            {
                _listeners.Remove(listener);
            }
        }
    }
}