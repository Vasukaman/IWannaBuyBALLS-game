// Filename: BallProximityVisuals.cs
using Gameplay.BallSystem;
using System.Linq;
using UnityEngine;

namespace VFX.BallEffects
{
    /// <summary>
    /// Controls a visual shader effect based on the proximity of other balls.
    /// It finds nearby balls and sends their data to the material.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class BallProximityVisuals : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float _proximityRadius = 1f;
        [SerializeField] private Color _proximityColor = Color.yellow;
        [SerializeField] private float _maxProximityStrength = 0.5f;
        [Tooltip("The maximum number of neighbors to send to the shader.")]
        [SerializeField] private int _maxNeighbors = 3;

        [Header("References")]
        // TODO: [Dependency] This component should not need a direct reference to the Ball's data model
        // just to get the factory. This creates unnecessary coupling.
        [SerializeField] private Ball _ball;

        // --- State & Cache ---
        private Renderer _renderer;
        private MaterialPropertyBlock _props;
        private Vector4[] _neighborPositions;
        private float[] _neighborStrengths;

        // --- Shader Property IDs ---
        private static readonly int NeighborPositionsID = Shader.PropertyToID("_NeighborPositions");
        private static readonly int NeighborStrengthsID = Shader.PropertyToID("_NeighborStrengths");
        private static readonly int NeighborCountID = Shader.PropertyToID("_NeighborCount");
        private static readonly int ProximityColorID = Shader.PropertyToID("_ProximityColor");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _props = new MaterialPropertyBlock();

            // Correctly initialize arrays based on the inspector setting.
            _neighborPositions = new Vector4[_maxNeighbors];
            _neighborStrengths = new float[_maxNeighbors];
        }

        // TODO: [Performance] Using LateUpdate() is inefficient. Use a timed InvokeRepeating or a Coroutine.
        //blah blah blah. It's fine, it's will look laggy if I NOT update it not every frame. Balls move EVERY FRAME.
        //I hope I'm right....

        private void LateUpdate()
        {
            // TODO: [SRP Violation] This component should not be responsible for querying the state of the entire game.
            // A physics-based query would be a more decoupled and performant solution.
            //I think I just need to do an injection here.
            if (_ball.BallFactory == null) return;

            FindAndSendNeighborData();
        }

        private void FindAndSendNeighborData()
        {
            System.Array.Clear(_neighborPositions, 0, _neighborPositions.Length);
            System.Array.Clear(_neighborStrengths, 0, _neighborStrengths.Length);

            var nearbyBalls = _ball.BallFactory.GetActiveBalls()
                .Where(b => b != null && b.gameObject != gameObject)
                .OrderBy(b => Vector3.Distance(transform.position, b.transform.position))
                .Take(_maxNeighbors)
                .ToList();

            for (int i = 0; i < nearbyBalls.Count; i++)
            {
                Vector3 pos = nearbyBalls[i].transform.position;
                float distance = Vector3.Distance(transform.position, pos);
                float strength = 1 - Mathf.Clamp01(distance / _proximityRadius);

                _neighborPositions[i] = new Vector4(pos.x, pos.y, pos.z, 0);
                _neighborStrengths[i] = strength * _maxProximityStrength;
            }

            _renderer.GetPropertyBlock(_props);
            _props.SetVectorArray(NeighborPositionsID, _neighborPositions);
            _props.SetFloatArray(NeighborStrengthsID, _neighborStrengths);
            _props.SetInt(NeighborCountID, nearbyBalls.Count);
            _props.SetColor(ProximityColorID, _proximityColor);
            _renderer.SetPropertyBlock(_props);
        }
    }
}