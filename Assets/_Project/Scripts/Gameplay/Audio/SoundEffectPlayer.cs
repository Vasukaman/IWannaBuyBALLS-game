// Filename: SoundEffectPlayer.cs
// Location: _Project/Scripts/Gameplay/Audio/
using Reflex.Attributes;

using UnityEngine;

public class SoundEffectPlayer : MonoBehaviour
{
    [Inject] private IAudioService _audioService;

    /// <summary>
    /// A public method that can be called by UnityEvents to play a sound
    /// defined in a SoundEffectProfile asset.
    /// </summary>
    public void Play(SoundEffectProfile sfxProfile)
    {
        _audioService.PlaySound(sfxProfile);
    }
}