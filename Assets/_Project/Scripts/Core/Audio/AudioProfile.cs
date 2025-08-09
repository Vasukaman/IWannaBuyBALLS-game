// Filename: AudioProfile.cs
// Location: _Project/Scripts/Core/Data/
using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioProfile", menuName = "Profiles/Core/Audio Profile")]
public class AudioProfile : ScriptableObject
{
    [Header("UI Sounds")]
    public SoundEffectProfile ButtonClick;

    [Header("Ball Sounds")]
    public SoundEffectProfile BallSpawn;
    public SoundEffectProfile BallMerge;
    public SoundEffectProfile BallSell;

    [Header("Gadget Sounds")]
    public SoundEffectProfile GadgetPlace;
}