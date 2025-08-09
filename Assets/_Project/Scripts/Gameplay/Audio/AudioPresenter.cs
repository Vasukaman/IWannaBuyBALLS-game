// Filename: AudioPresenter.cs
// Location: _Project/Scripts/Core/
using Core.Events;
using Reflex.Attributes;
using UnityEngine;
using Core.Interfaces;
using Core.Data;
public class AudioPresenter : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AudioProfile _audioProfile;

    [Header("Event Channels")]
    [SerializeField] private EventChannels _eventChannels;

    [Inject] private IAudioService _audioService;

    private void OnEnable()
    {
        // Subscribe to all the events you want to have sounds
        _eventChannels.OnBallSold.RegisterListener(HandleBallSold);
        _eventChannels.OnBallSpawned.RegisterListener(HandleBallSpawned);
        _eventChannels.OnBallMerged.RegisterListener(HandleBallMerged);
        _eventChannels.OnManualActivation.RegisterListener(HandleManualActivator);
        _eventChannels.OnGadgetPlaced.RegisterListener(HandleGadgetPlaced);
        _eventChannels.OnBallCollisionSound.RegisterListener(HandleCollisionSound);
        // You would add OnBallMerged here later
    }

    private void OnDisable()
    {
        _eventChannels.OnBallSold.UnregisterListener(HandleBallSold);
        _eventChannels.OnBallSpawned.UnregisterListener(HandleBallSpawned);
        _eventChannels.OnBallMerged.UnregisterListener(HandleBallMerged);
        _eventChannels.OnManualActivation.UnregisterListener(HandleManualActivator);
        _eventChannels.OnGadgetPlaced.UnregisterListener(HandleGadgetPlaced);
        _eventChannels.OnBallCollisionSound.UnregisterListener(HandleCollisionSound);
    }

    private void HandleBallSold(int amount)
    {
        _audioService.PlaySound(_audioProfile.BallSell);
    }

    private void HandleBallSpawned()
    {
        _audioService.PlaySound(_audioProfile.BallSpawn);
    }

    private void HandleBallMerged(IBallView ball)
    {
        _audioService.PlaySound(_audioProfile.BallMerge);
    }

    private void HandleManualActivator()
    {
        _audioService.PlaySound(_audioProfile.ButtonClick);
    }

    private void HandleGadgetPlaced(GadgetData data)
    {
        _audioService.PlaySound(_audioProfile.GadgetPlace);
    }

    private void HandleCollisionSound(SoundEffectProfile sfx)
    {
        // The presenter's job is simple: just tell the service to play the sound.
        _audioService.PlaySound(sfx);
    }

}