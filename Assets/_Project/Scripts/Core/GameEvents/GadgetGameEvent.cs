// Filename: IntGameEvent.cs
using Core.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Data;
using static UnityEngine.GraphicsBuffer;

namespace Core.Events
{
    /// <summary>
    /// A ScriptableObject that acts as a global event channel for events
    /// that need to pass an integer value as a parameter.
    /// </summary>
    [CreateAssetMenu(fileName = "NewGadgetGameEvent", menuName = "Game Events/Gadget Game Event")]
    public class GadgetGameEvent : ScriptableObject
    {
        private readonly List<Action<GadgetData>> _listeners = new List<Action<GadgetData>>();

        public void Raise(GadgetData value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i]?.Invoke(value);
            }
        }

        public void RegisterListener(Action<GadgetData> listener)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        public void UnregisterListener(Action<GadgetData> listener)
        {
            if (_listeners.Contains(listener))
            {
                _listeners.Remove(listener);
            }
        }
    }
}