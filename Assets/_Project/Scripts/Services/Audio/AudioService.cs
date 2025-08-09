// Filename: AudioService.cs
using Core.Spawning;
using UnityEngine;

public class AudioService : IAudioService
{
    private readonly SoundSettingsProfile _settings;
    private readonly IPrefabInstantiator _instantiator;
    private readonly GameObject _audioSourcePrefab;
    private AudioSource _audioSource;

    // The constructor now takes the instantiator and the blueprint prefab.
    public AudioService(SoundSettingsProfile settings, IPrefabInstantiator instantiator, GameObject audioSourcePrefab)
    {
        _settings = settings;
        _instantiator = instantiator;
        _audioSourcePrefab = audioSourcePrefab;
    }

    public void PlaySound(SoundEffectProfile sfx)
    {
        if (sfx == null || sfx.Clip == null) return;

        // The service ensures its AudioSource is valid before using it.
        if (GetValidAudioSource())
        {
            _audioSource.volume = _settings.MasterVolume * _settings.SfxVolume *
                                  Random.Range(sfx.Volume - sfx.VolumeVariation, sfx.Volume + sfx.VolumeVariation);

            _audioSource.pitch = Random.Range(1f - sfx.PitchVariation, 1f + sfx.PitchVariation);

            _audioSource.PlayOneShot(sfx.Clip);
        }
    }

    /// <summary>
    /// This is the "self-healing" method. It uses the instantiator to create the AudioSource.
    /// </summary>
    private bool GetValidAudioSource()
    {
        if (_audioSource != null)
        {
            return true; // We already have a valid source.
        }

        // Ask the "worker" to create an instance of our blueprint.
        GameObject audioGameObject = _instantiator.InstantiatePrefab(_audioSourcePrefab, Vector3.zero);
        audioGameObject.name = "[AudioSource_Runtime]"; // Give it a clear name in the hierarchy

        // Make it persistent.
        Object.DontDestroyOnLoad(audioGameObject);

        _audioSource = audioGameObject.GetComponent<AudioSource>();

        return _audioSource != null;
    }
}