// Filename: WaveTrailRenderer.cs
using UnityEngine;

//YEYOO It's now using instancing, let's go optimisation!
namespace BallMania.VFX
{
    public class WaveTrailRenderer : MonoBehaviour
    {
        [Header("Configuration")]
        public WaveTrailProfile styleProfile;
        public Transform target;

        [Header("Internal")]
        [SerializeField] private Mesh _dotMesh;
        [SerializeField] private Material _dotMaterial;

        private const int MAX_DOTS = 1023;
        private Matrix4x4[] _matrices = new Matrix4x4[MAX_DOTS];
        private Vector4[] _data = new Vector4[MAX_DOTS];
        private Vector4[] _targetData = new Vector4[MAX_DOTS];
        private Vector4[] _startParams = new Vector4[MAX_DOTS];
        private Vector4[] _endParams = new Vector4[MAX_DOTS];
        private Vector4[] _colors = new Vector4[MAX_DOTS];
        private MaterialPropertyBlock _props;

        private int _currentDotIndex = 0;
        private float _lastSpawnTime;

        void Start()
        {
            _props = new MaterialPropertyBlock();
            for (int i = 0; i < MAX_DOTS; i++) _matrices[i] = Matrix4x4.identity;
        }

        void Update()
        {
            if (styleProfile == null || target == null || _dotMesh == null || _dotMaterial == null) return;

            if (Time.time > _lastSpawnTime + styleProfile.spawnInterval)
            {
                SpawnDot();
                _lastSpawnTime = Time.time;
            }

            UpdateAndRenderInstances();
        }

        private void SpawnDot()
        {
           
            Vector2 startPos = transform.position;
            Vector2 endPos = target.position;
            float distance = Vector2.Distance(startPos, endPos);

            // Calculate speed for this specific dot, including randomness
            float speed = styleProfile.dotSpeed + Random.Range(-styleProfile.speedDeviation, styleProfile.speedDeviation);
            speed = Mathf.Max(0.01f, speed); // Ensure speed is not zero to avoid errors

            // Calculate lifetime based on distance and speed
            float lifetime = distance / speed;
        

            // Apply randomness to other base values
            float amp = styleProfile.amplitude + Random.Range(-styleProfile.amplitudeDeviation, styleProfile.amplitudeDeviation);
            float freq = styleProfile.frequency + Random.Range(-styleProfile.frequencyDeviation, styleProfile.frequencyDeviation);
            float scale = styleProfile.scale + Random.Range(-styleProfile.scaleDeviation, styleProfile.scaleDeviation);

            // Calculate final start and end values using multipliers
            float startAmp = amp * styleProfile.startAmplitudeMultiplier;
            float endAmp = amp * styleProfile.endAmplitudeMultiplier;

            float startFreq = freq * styleProfile.startFrequencyMultiplier;
            float endFreq = freq * styleProfile.endFrequencyMultiplier;

            float startScale = scale * styleProfile.startScaleMultiplier;
            float endScale = scale * styleProfile.endScaleMultiplier;

            // Pack data for the shader (now using the calculated lifetime)
            _data[_currentDotIndex] = new Vector4(startPos.x, startPos.y, Time.time, lifetime);
            _targetData[_currentDotIndex] = new Vector4(endPos.x, endPos.y, 0, 0);
            _startParams[_currentDotIndex] = new Vector4(startAmp, startFreq, startScale, 0);
            _endParams[_currentDotIndex] = new Vector4(endAmp, endFreq, endScale, 0);
            _colors[_currentDotIndex] = styleProfile.color;

            // Move to the next dot index
            _currentDotIndex = (_currentDotIndex + 1) % MAX_DOTS;
        }

        private void UpdateAndRenderInstances()
        {
            // Set the per-instance data arrays
            _props.SetVectorArray("_Data", _data);
            _props.SetVectorArray("_Target", _targetData);
            _props.SetVectorArray("_StartParams", _startParams);
            _props.SetVectorArray("_EndParams", _endParams);
            _props.SetVectorArray("_Color", _colors);

            // Set the single global wave speed property
            _props.SetFloat("_GlobalWaveSpeed", styleProfile.globalWaveSpeed);

            Graphics.DrawMeshInstanced(
                _dotMesh, 0, _dotMaterial, _matrices, MAX_DOTS, _props
            );
        }
    }
}