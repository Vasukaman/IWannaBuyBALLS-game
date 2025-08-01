// Filename: BallProximityVisuals.cs
using Gameplay.BallSystem;
using UnityEngine;

namespace VFX.BallEffects
{
    /// <summary>
    /// Controls a visual shader effect based on the proximity of other balls.
    /// It uses an efficient physics query to find nearby balls and sends their data to the material.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class BallProximityVisuals : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The radius within which to detect other balls.")]
        [SerializeField] private float _proximityRadius = 2f;
        [Tooltip("The maximum number of neighbors to send to the shader. MUST match the shader array size.")]
        [SerializeField] private int _maxNeighbors = 4;

        // --- State & Cache ---
        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;

        // Data arrays to be sent to the shader
        private Vector4[] _neighborPositions;
        private Vector4[] _neighborColors; // Assuming the shader needs color too

        // A pre-allocated buffer for the physics query to prevent generating garbage memory.
        private Collider2D[] _queryResults;

        // --- Shader Property IDs ---
        // This component fulfills a contract with a shader that expects the following properties:
        // - uniform int _NeighborCount;
        // - uniform float4 _NeighborPositions[MAX_NEIGHBORS]; // (x, y, z=radius, w=unused)
        // - uniform fixed4 _NeighborColors[MAX_NEIGHBORS];
        private static readonly int NeighborPositionsID = Shader.PropertyToID("_NeighborPositions");
        private static readonly int NeighborColorsID = Shader.PropertyToID("_NeighborColors");
        private static readonly int NeighborCountID = Shader.PropertyToID("_NeighborCount");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();

            // Initialize all arrays to the correct size based on the inspector setting.
            _neighborPositions = new Vector4[_maxNeighbors];
            _neighborColors = new Vector4[_maxNeighbors];
            _queryResults = new Collider2D[_maxNeighbors + 1]; // +1 to account for possibly hitting our own collider
        }

        private void LateUpdate()
        {
            // The entire process is now self-contained and much more efficient.
            FindNeighborsAndApplyVisuals();
        }

        /// <summary>
        /// Uses a non-allocating physics circle query to find nearby balls and updates the shader.
        /// </summary>
        private void FindNeighborsAndApplyVisuals()
        {
            // This is the key performance improvement. Instead of iterating all balls,
            // we ask the physics engine for only the colliders within our radius.
            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, _proximityRadius, _queryResults);

            int neighborCount = 0;
            for (int i = 0; i < hitCount && neighborCount < _maxNeighbors; i++)
            {
                // Skip our own collider.
                if (_queryResults[i].gameObject == gameObject) continue;

                // Check if the object found is a ball.
                if (_queryResults[i].TryGetComponent<BallView>(out var neighborBall))
                {
                    // Populate the data arrays for the shader.
                    _neighborPositions[neighborCount] = new Vector4(
                        neighborBall.transform.position.x,
                        neighborBall.transform.position.y,
                        neighborBall.Radius, // Send the actual radius for more accurate effects
                        0
                    );
                    _neighborColors[neighborCount] = neighborBall.Color;
                    neighborCount++;
                }
            }

            // Apply the collected data to the renderer's material property block.
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetInt(NeighborCountID, neighborCount);
            if (neighborCount > 0)
            {
                _propertyBlock.SetVectorArray(NeighborPositionsID, _neighborPositions);
                _propertyBlock.SetVectorArray(NeighborColorsID, _neighborColors);
            }
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}