// Filename: AudioProfile.cs
// Location: _Project/Scripts/Core/Data/
using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioProfile", menuName = "Profiles/Core/Audio Profile")]
public class AudioProfile : ScriptableObject
{
    [Header("UI Sounds")]
    public SoundEffect ButtonClick;

    [Header("Ball Sounds")]
    public SoundEffect BallSpawn;
    public SoundEffect BallMerge;
    public SoundEffect BallSell;

    [Header("Gadget Sounds")]
    public SoundEffect GadgetPlace;
}