// Filename: BallMerger.cs
using Gameplay.BallSystem;
using System.Collections;
using UnityEngine;
using Core;


// I definetty could have written this better. After asking AI for architectural mistakes it said it HATES this scipt.
//Yeah, acessing the other ball's merger is kinda bad, but I think it's ok solution for now.
//I'm pretty sure it CAN cause problems if I'm gonna do anything else with merging, BUT I'm not in close future.
//So I think it's fine to fix later.
    
namespace Gameplay.BallSystem
{
    /// <summary>
    /// Handles the logic and visual effect for merging two balls of the same price.
    /// </summary>
    [RequireComponent(typeof(Ball))]
    public class BallMerger : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool _allowMerge = true;
        [SerializeField] private float _mergeDuration = 0.35f;
        [SerializeField] private float _mergeCooldownAfterSpawn = 0.5f;
        [SerializeField] private float _maxVelocityToMerge = 5f;

        [Header("Visualization")]
        [SerializeField] private Renderer _ballRenderer;
        [SerializeField] private float _visualRadiusMultiplier = 0.8f;
        [SerializeField] private float _positionCorrectionFactor = 1.2f;

        // --- State ---
        private Ball _ball;
        private Rigidbody2D _rb;
        private MaterialPropertyBlock _mpb;
        private bool _isReadyForMerge = false;

        // --- Shader Property IDs (Cached for performance) ---
        private static readonly int MergeTargetPosID = Shader.PropertyToID("_MergeTargetPos");
        private static readonly int MergeTargetRadiusID = Shader.PropertyToID("_MergeTargetRadius");
        private static readonly int MergeWeightID = Shader.PropertyToID("_MergeWeight");

        // --- Unity Methods ---

        private void Awake()
        {
            _ball = GetComponent<Ball>();
            _rb = GetComponent<Rigidbody2D>();

            if (_ballRenderer != null)
            {
                _mpb = new MaterialPropertyBlock();
                _ballRenderer.GetPropertyBlock(_mpb);
            }

            _ball.OnInitialize += HandleBallInitialized;
        }

        private void OnDestroy()
        {
            if (_ball != null)
                _ball.OnInitialize -= HandleBallInitialized;
        }

        // --- Public API ---

        /// <summary>
        /// Called by the BallCollisionHandler to attempt a merge with another ball.
        /// </summary>
        public void InitiateMergeWith(Ball other)
        {
            if (!CanMergeWith(other)) return;

            // TODO: [Encapsulation Violation] This component reaches across to another BallMerger to modify its state.
            // A better architecture would use a central event bus or have the Ball class mediate,
            // for example: `other.StartMerge(this._ball);` which would handle its own internal state.
            var otherMerger = other.GetComponent<BallMerger>();
            if (otherMerger == null || !otherMerger._isReadyForMerge) return;

            // Prevent both balls from trying to merge again until this one is complete
            otherMerger._isReadyForMerge = false;
            _isReadyForMerge = false;

            StartCoroutine(MergeCoroutine(other));
        }

        // --- Event Handlers ---

        /// <summary>
        /// Resets the merge state when the ball is initialized (e.g., from a pool).
        /// </summary>
        private void HandleBallInitialized(Ball ball)
        {
            _isReadyForMerge = false;
            StopAllCoroutines(); // Ensure no old coroutines are running on this pooled object
            StartCoroutine(EnableMergeAfterCooldown());
        }

        // --- Private Logic & Coroutines ---

        /// <summary>
        /// Checks all pre-conditions to determine if a merge is possible.
        /// </summary>
        private bool CanMergeWith(Ball other)
        {
            // TODO: [Coupling] This directly gets a component from the 'other' ball.
            // A better approach would be to pass the velocity in the InitiateMergeWith call,
            // or have the Ball class expose a 'Velocity' property.
            var otherRb = other.GetComponent<Rigidbody2D>();
            if (otherRb == null) return false;

            return _allowMerge &&
                   _isReadyForMerge &&
                   _ball.CurrentPrice == other.CurrentPrice &&
                   _rb.velocity.magnitude <= _maxVelocityToMerge &&
                   otherRb.velocity.magnitude <= _maxVelocityToMerge &&
                   // Use InstanceID to ensure only one of two colliding balls initiates the merge.
                   GetInstanceID() > other.GetInstanceID();
        }

        private IEnumerator EnableMergeAfterCooldown()
        {
            yield return new WaitForSeconds(_mergeCooldownAfterSpawn);
            _isReadyForMerge = true;
        }

        private IEnumerator MergeCoroutine(Ball other)
        {
            // TODO: [Magic Numbers] Layer indices are hardcoded. These should be defined in a
            // static class like 'GameLayers.cs' to avoid errors and improve readability.
            gameObject.layer = GameLayers.MergingBall; // Set to "MergingBall" layer
            other.gameObject.layer = GameLayers.MergingBall;

            // TODO: [Encapsulation Violation] Directly gets the other merger component again.
            var otherMerger = other.GetComponent<BallMerger>();
            if (otherMerger == null)
            {
                // Failsafe if the other ball somehow lost its merger component mid-process
                FinalizeMerge(other);
                yield break;
            }

            float elapsedTime = 0f;
            Vector3 thisStartPosition = transform.position;
            Vector3 otherStartPosition = other.transform.position;

            while (elapsedTime < _mergeDuration)
            {
                elapsedTime += Time.deltaTime;
                float weight = Mathf.SmoothStep(0, 1, elapsedTime / _mergeDuration);
                Vector3 mergeCenter = Vector3.Lerp(thisStartPosition, otherStartPosition, 0.5f);

                // Animate both balls towards the center
                transform.position = Vector3.Lerp(thisStartPosition, mergeCenter, weight);
                other.transform.position = Vector3.Lerp(otherStartPosition, mergeCenter, weight);

                // Update shaders for the visual effect
                UpdateMergeShader(other);
                otherMerger.UpdateMergeShader(_ball);

                yield return null;
            }

            FinalizeMerge(other);
        }

        /// <summary>
        /// Completes the merge process: updates price, despawns the other ball, and resets state.
        /// </summary>
        private void FinalizeMerge(Ball other)
        {
            _ball.MultiplyPrice(2f);
            other.Despawn();

            gameObject.layer = GameLayers.Ball; // Reset to "Ball" layer
            other.gameObject.layer = GameLayers.Ball; // Reset to "Ball" layer
            _isReadyForMerge = true;

            ResetMergeShader();

            // TODO: [Encapsulation Violation] Should not be responsible for resetting another object's shader.
            // The 'other' ball should handle its own reset via its OnDespawned event.
            var otherMerger = other.GetComponent<BallMerger>();
            if (otherMerger != null)
            {
                otherMerger.ResetMergeShader();
            }
        }

        // --- Shader Update Methods ---

        /// <summary>
        /// Updates this ball's material properties to visualize the merge.
        /// </summary>
        public void UpdateMergeShader(Ball other)
        {
            if (_ballRenderer == null) return;

            float otherWorldRadius = other.Radius * _visualRadiusMultiplier;
            Vector3 otherLocalPos = transform.InverseTransformPoint(other.transform.position);
            Vector2 otherUV = new Vector2(
                0.5f + (otherLocalPos.x * _positionCorrectionFactor) / transform.lossyScale.x,
                0.5f + (otherLocalPos.y * _positionCorrectionFactor) / transform.lossyScale.y
            );
            float uvRadius = otherWorldRadius / Mathf.Max(transform.lossyScale.x, 0.001f);

            _mpb.SetVector(MergeTargetPosID, otherUV);
            _mpb.SetFloat(MergeTargetRadiusID, uvRadius);
            _mpb.SetFloat(MergeWeightID, 1); // Weight is 1 during the merge visual
            _ballRenderer.SetPropertyBlock(_mpb);
        }

        /// <summary>
        /// Resets the merge effect on this ball's material.
        /// </summary>
        public void ResetMergeShader()
        {
            if (_ballRenderer == null) return;
            _mpb.SetFloat(MergeWeightID, 0);
            _ballRenderer.SetPropertyBlock(_mpb);
        }
    }
}