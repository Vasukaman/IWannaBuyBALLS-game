// Filename: WaveTrailView.cs
using BallMania.VFX;
using Gameplay.Interfaces;
using UnityEngine;
using VFX.Interfaces;
namespace VFX
{
    /// <summary>
    /// The "View" for a wave trail effect. This component is responsible for all rendering,
    /// using GPU instancing for high performance. It is a "dumb" component controlled by a Presenter.
    /// </summary>
    public class WaveTrailView : MonoBehaviour, ITargetableVisual
    {
        [Header("Internal References")]
        [SerializeField] private Mesh _dotMesh;
        [SerializeField] private Material _dotMaterial;

        // --- Public Properties (Controlled by a Presenter) ---
        public WaveTrailProfile StyleProfile;
        public Transform Target { get; set; }

        // --- Instancing Data ---
        private const int MAX_DOTS = 1023;
        private readonly Matrix4x4[] _matrices = new Matrix4x4[MAX_DOTS];
        private readonly Vector4[] _data = new Vector4[MAX_DOTS];
        private readonly Vector4[] _targetData = new Vector4[MAX_DOTS];
        private readonly Vector4[] _startParams = new Vector4[MAX_DOTS];
        private readonly Vector4[] _endParams = new Vector4[MAX_DOTS];
        private readonly Vector4[] _colors = new Vector4[MAX_DOTS];
        private MaterialPropertyBlock _props;

        private int _currentDotIndex = 0;
        private float _lastSpawnTime;

        private void Awake()
        {
            _props = new MaterialPropertyBlock();
            for (int i = 0; i < MAX_DOTS; i++) _matrices[i] = Matrix4x4.identity;
        }

        private void Update()
        {
            if (StyleProfile == null || Target == null || _dotMesh == null || _dotMaterial == null) return;

            if (Time.time > _lastSpawnTime + StyleProfile.spawnInterval)
            {
                SpawnDot();
                _lastSpawnTime = Time.time;
            }

            UpdateAndRenderInstances();
        }

        private void SpawnDot()
        {
            Vector2 startPos = transform.position;
            Vector2 endPos = Target.position;
            float distance = Vector2.Distance(startPos, endPos);

            float speed = StyleProfile.dotSpeed + Random.Range(-StyleProfile.speedDeviation, StyleProfile.speedDeviation);
            speed = Mathf.Max(0.01f, speed);
            float lifetime = distance / speed;

            float amp = StyleProfile.amplitude + Random.Range(-StyleProfile.amplitudeDeviation, StyleProfile.amplitudeDeviation);
            float freq = StyleProfile.frequency + Random.Range(-StyleProfile.frequencyDeviation, StyleProfile.frequencyDeviation);
            float scale = StyleProfile.scale + Random.Range(-StyleProfile.scaleDeviation, StyleProfile.scaleDeviation);

            float startAmp = amp * StyleProfile.startAmplitudeMultiplier;
            float endAmp = amp * StyleProfile.endAmplitudeMultiplier;
            float startFreq = freq * StyleProfile.startFrequencyMultiplier;
            float endFreq = freq * StyleProfile.endFrequencyMultiplier;
            float startScale = scale * StyleProfile.startScaleMultiplier;
            float endScale = scale * StyleProfile.endScaleMultiplier;

            _data[_currentDotIndex] = new Vector4(startPos.x, startPos.y, Time.time, lifetime);
            _targetData[_currentDotIndex] = new Vector4(endPos.x, endPos.y, 0, 0);
            _startParams[_currentDotIndex] = new Vector4(startAmp, startFreq, startScale, 0);
            _endParams[_currentDotIndex] = new Vector4(endAmp, endFreq, endScale, 0);
            _colors[_currentDotIndex] = StyleProfile.color;

            _currentDotIndex = (_currentDotIndex + 1) % MAX_DOTS;
        }

        private void UpdateAndRenderInstances()
        {
            _props.SetVectorArray("_Data", _data);
            _props.SetVectorArray("_Target", _targetData);
            _props.SetVectorArray("_StartParams", _startParams);
            _props.SetVectorArray("_EndParams", _endParams);
            _props.SetVectorArray("_Color", _colors);
            _props.SetFloat("_GlobalWaveSpeed", StyleProfile.globalWaveSpeed);

            Graphics.DrawMeshInstanced(_dotMesh, 0, _dotMaterial, _matrices, MAX_DOTS, _props);
        }
    }
}