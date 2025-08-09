// Filename: SoundEffectProfile.cs
// Location: _Project/Scripts/Core/Audio/
using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundEffect", menuName = "Profiles/Core/Sound Effect")]
public class SoundEffectProfile : ScriptableObject
{
    public AudioClip Clip;

    [Range(0f, 1f)]
    public float Volume = 1f;

    [Range(0f, 0.5f)]
    public float VolumeVariation = 0.1f;

    [Range(0f, 0.5f)]
    public float PitchVariation = 0.1f;
}