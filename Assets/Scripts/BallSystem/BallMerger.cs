// Filename: BallMerger.cs
using Core;
using Gameplay.BallSystem;
using System.Collections;
using UnityEngine;

namespace Gameplay.BallSystem
{
    /// <summary>
    /// Handles the logic and visual effect for merging two balls of the same price.
    /// </summary>
    [RequireComponent(typeof(BallView))]
    public class BallMerger : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float _mergeDuration = 0.35f;
        [SerializeField] private float _mergeCooldownAfterSpawn = 0.5f;
        [SerializeField] private float _maxVelocityToMerge = 5f;

        [Header("Visualization")]
        [SerializeField] private Renderer _ballRenderer;
        [SerializeField] private float _visualRadiusMultiplier = 0.8f;
        [SerializeField] private float _positionCorrectionFactor = 1.2f;

        // --- State & Cache ---
        private BallView _ballView;
        private MaterialPropertyBlock _propertyBlock;

        // --- Shader Property IDs ---
        private static readonly int MergeTargetPosID = Shader.PropertyToID("_MergeTargetPos");
        private static readonly int MergeTargetRadiusID = Shader.PropertyToID("_MergeTargetRadius");
        private static readonly int MergeWeightID = Shader.PropertyToID("_MergeWeight");

        // --- Unity Methods ---

        private void Awake()
        {
            _ballView = GetComponent<BallView>();

            if (_ballRenderer != null)
            {
                _propertyBlock = new MaterialPropertyBlock();
                _ballRenderer.GetPropertyBlock(_propertyBlock);
            }
        }

        private void OnEnable()
        {
            if (_ballView != null)
            {
                _ballView.OnInitialize += HandleBallInitialized;
                _ballView.OnDespawned += HandleBallDespawned;
            }
        }

        private void OnDisable()
        {
            if (_ballView != null)
            {
                _ballView.OnInitialize -= HandleBallInitialized;
                _ballView.OnDespawned -= HandleBallDespawned;
            }
        }

        // --- Public API ---

        /// <summary>
        /// Called by the BallCollisionHandler to attempt a merge with another ball.
        /// </summary>
        public void InitiateMergeWith(BallView otherBall)
        {
            if (!CanMergeWith(otherBall)) return;

            // Set the state on the public API of both BallViews. This is clean and decoupled.
            _ballView.CanMerge = false;
            otherBall.CanMerge = false;

            StartCoroutine(MergeCoroutine(otherBall));
        }

        // --- Event Handlers ---

        private void HandleBallInitialized(BallView ball)
        {
            // The ball is not ready to merge immediately after spawning.
            _ballView.CanMerge = false;
            StopAllCoroutines();
            StartCoroutine(EnableMergeAfterCooldown());
        }

        /// <summary>
        /// When our ball despawns, ensure its shader is reset.
        /// </summary>
        private void HandleBallDespawned(BallView ball)
        {
            ResetMergeShader();
        }

        // --- Private Logic & Coroutines ---

        private bool CanMergeWith(BallView otherBall)
        {
            return _ballView.CanMerge &&
                   otherBall.CanMerge &&
                   _ballView.Data.CurrentPrice == otherBall.Data.CurrentPrice &&
                   _ballView.Velocity <= _maxVelocityToMerge &&
                   otherBall.Velocity <= _maxVelocityToMerge &&
                   // Use InstanceID to ensure only one of two colliding balls initiates the merge.
                   GetInstanceID() > otherBall.GetInstanceID();
        }

        private IEnumerator EnableMergeAfterCooldown()
        {
            yield return new WaitForSeconds(_mergeCooldownAfterSpawn);
            if (this != null) // Failsafe in case the object was destroyed during the wait
            {
                _ballView.CanMerge = true;
            }
        }

        private IEnumerator MergeCoroutine(BallView otherBall)
        {
            var originalLayer = gameObject.layer;
            gameObject.layer = GameLayers.MergingBall;
            otherBall.gameObject.layer = GameLayers.MergingBall;

            var otherMerger = otherBall.GetComponent<BallMerger>();
            if (otherMerger == null)
            {
                FinalizeMerge(otherBall, originalLayer);
                yield break;
            }

            float elapsedTime = 0f;
            Vector3 thisStartPosition = transform.position;
            Vector3 otherStartPosition = otherBall.transform.position;

            while (elapsedTime < _mergeDuration)
            {
                // Failsafe in case the other ball is destroyed mid-animation
                if (otherBall == null) { FinalizeMerge(null, originalLayer); yield break; }

                elapsedTime += Time.deltaTime;
                float weight = Mathf.SmoothStep(0, 1, elapsedTime / _mergeDuration);
                Vector3 mergeCenter = Vector3.Lerp(thisStartPosition, otherStartPosition, 0.5f);

                transform.position = Vector3.Lerp(thisStartPosition, mergeCenter, weight);
                otherBall.transform.position = Vector3.Lerp(otherStartPosition, mergeCenter, weight);

                UpdateMergeShader(otherBall);
                otherMerger.UpdateMergeShader(_ballView);

                yield return null;
            }

            FinalizeMerge(otherBall, originalLayer);
        }

        private void FinalizeMerge(BallView otherBall, int originalLayer)
        {
            // Only modify our ball and despawn the other.
            if (otherBall != null)
            {
                _ballView.Data.MultiplyPrice(2f);
                otherBall.Despawn();
            }

            gameObject.layer = originalLayer;
            ResetMergeShader();

            // Start the cooldown again for this newly merged ball.
            StartCoroutine(EnableMergeAfterCooldown());
        }

        // --- Shader Update Methods ---

        public void UpdateMergeShader(BallView otherBall)
        {
            if (_ballRenderer == null) return;

            float otherWorldRadius = otherBall.Radius * _visualRadiusMultiplier;
            Vector3 otherLocalPos = transform.InverseTransformPoint(otherBall.transform.position);
            Vector2 otherUV = new Vector2(
                0.5f + (otherLocalPos.x * _positionCorrectionFactor) / transform.lossyScale.x,
                0.5f + (otherLocalPos.y * _positionCorrectionFactor) / transform.lossyScale.y
            );
            float uvRadius = otherWorldRadius / Mathf.Max(transform.lossyScale.x, 0.001f);

            _propertyBlock.SetVector(MergeTargetPosID, otherUV);
            _propertyBlock.SetFloat(MergeTargetRadiusID, uvRadius);
            _propertyBlock.SetFloat(MergeWeightID, 1);
            _ballRenderer.SetPropertyBlock(_propertyBlock);
        }

        public void ResetMergeShader()
        {
            if (_ballRenderer == null) return;
            _propertyBlock.SetFloat(MergeWeightID, 0);
            _ballRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}