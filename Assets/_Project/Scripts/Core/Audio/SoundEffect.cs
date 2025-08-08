// Filename: SoundEffect.cs
// Location: _Project/Scripts/Core/Audio/
using UnityEngine;

[System.Serializable]
public class SoundEffect
{
    public AudioClip Clip;

    [Range(0f, 1f)]
    public float Volume = 1f;

    [Range(0f, 0.5f)]
    public float VolumeVariation = 0.1f;

    [Range(0f, 0.5f)]
    public float PitchVariation = 0.1f;
}