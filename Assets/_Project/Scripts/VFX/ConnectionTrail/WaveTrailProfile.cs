// Filename: WaveTrailProfile.cs
using UnityEngine;

namespace BallMania.VFX
{
    [CreateAssetMenu(fileName = "NewWaveTrailProfile", menuName = "VFX/Wave Trail Profile")]
    public class WaveTrailProfile : ScriptableObject
    {
        [Header("Spawning & Movement")]
        public float spawnInterval = 0.1f;
        [Tooltip("How fast dots travel towards the target in units per second.")]
        public float dotSpeed = 5.0f; // REPLACED dotLifetime

        [Header("Base Parameters")]
        public Color color = Color.white;
        public float amplitude = 0.2f;
        public float frequency = 2f;
        public float scale = 0.2f;
        public float globalWaveSpeed = 5.0f;

        [Header("Randomness (Applied at Spawn)")]
        [Range(0f, 1f)] public float amplitudeDeviation = 0.05f;
        [Range(0f, 5f)] public float frequencyDeviation = 0.5f;
        [Range(0f, 5f)] public float speedDeviation = 1.0f; // REPLACED lifetimeDeviation
        [Range(0f, 0.5f)] public float scaleDeviation = 0.05f;

        [Header("Gradual Change Over Dot Lifetime")]
        public float startAmplitudeMultiplier = 1.0f;
        public float endAmplitudeMultiplier = 0.5f;
        [Space]
        public float startFrequencyMultiplier = 1.0f;
        public float endFrequencyMultiplier = 2.0f;
        [Space]
        public float startScaleMultiplier = 1.0f;
        public float endScaleMultiplier = 0.0f;
    }
}