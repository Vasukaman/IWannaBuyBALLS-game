// Filename: SoundSettingsProfile.cs
// Location: _Project/Scripts/Core/Audio/
using UnityEngine;

[CreateAssetMenu(fileName = "SoundSettings", menuName = "Profiles/Core/Sound Settings Profile")]
public class SoundSettingsProfile : ScriptableObject
{
    [Header("Master Volume")]
    [Range(0f, 1f)]
    public float MasterVolume = 1f;

    [Range(0f, 1f)]
    public float SfxVolume = 0.8f;

    [Range(0f, 1f)]
    public float MusicVolume = 0.6f;
}