// Filename: SoundEffectGameEvent.cs
// Location: _Project/Scripts/Core/Events/
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundEffectEvent", menuName = "Game Events/Sound Effect Event")]
public class SoundEffectGameEvent : ScriptableObject
{
    private readonly List<Action<SoundEffectProfile>> _listeners = new List<Action<SoundEffectProfile>>();

    public void Raise(SoundEffectProfile sfx)
    {
        for (int i = _listeners.Count - 1; i >= 0; i--)
        {
            _listeners[i]?.Invoke(sfx);
        }
    }

    public void RegisterListener(Action<SoundEffectProfile> listener)
    {
        if (!_listeners.Contains(listener)) _listeners.Add(listener);
    }

    public void UnregisterListener(Action<SoundEffectProfile> listener)
    {
        if (_listeners.Contains(listener)) _listeners.Remove(listener);
    }
}