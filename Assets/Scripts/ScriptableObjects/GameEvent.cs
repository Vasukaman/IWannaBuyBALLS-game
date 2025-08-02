// Filename: GameEvent.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Events
{
    /// <summary>
    /// A ScriptableObject that acts as a global event channel.
    /// Components can reference this asset to raise events or listen for them
    /// without having any direct knowledge of each other.
    /// </summary>
    [CreateAssetMenu(fileName = "NewGameEvent", menuName = "Game Events/Game Event")]
    public class GameEvent : ScriptableObject
    {
        // Using a List of Actions is more robust than a single Action
        // in case a listener is destroyed without unregistering.
        private readonly List<Action> _listeners = new List<Action>();

        public void Raise()
        {
            // Loop backwards so we can safely remove listeners if they are null
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke();
            }
        }

        public void RegisterListener(Action listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(Action listener)
        {
            if (_listeners.Contains(listener))
            {
                _listeners.Remove(listener);
            }
        }
    }
}