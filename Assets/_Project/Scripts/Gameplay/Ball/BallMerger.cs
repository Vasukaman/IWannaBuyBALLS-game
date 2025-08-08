// Filename: BallMerger.cs
using Core;
using Core.Events;
using Gameplay.BallSystem;
using Reflex.Attributes;
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
        
        [Header("References")]
        [SerializeField] private Renderer _ballRenderer;
        [SerializeField] private BallGameEvent _onBallMerge;

        // --- References ---
        private BallView _ballView;
        private BallMergingProfile _mergingProfile;

        private MaterialPropertyBlock _propertyBlock;

        [Inject] private SoundSettingsProfile _soundSettingsProfile;

        // --- Shader Property IDs ---
        private static readonly int MergeTargetPosID = Shader.PropertyToID("_MergeTargetPos");
        private static readonly int MergeTargetRadiusID = Shader.PropertyToID("_MergeTargetRadius");
        private static readonly int MergeWeightID = Shader.PropertyToID("_MergeWeight");

        // --- Unity Methods ---

        private void Awake()
        {
            // 1. Get the root component.
            _ballView = GetComponent<BallView>();

            // 2. Pull the specific profile.
            if (_ballView.Profile != null)
            {
                _mergingProfile = _ballView.Profile.Merging;
            }

            // 3. Validate.
            if (_mergingProfile == null)
            {
                Debug.LogError("BallMergingProfile is not assigned in the master BallProfile! Disabling BallMerger.", this);
                enabled = false;
                return; // Return early to prevent further null errors
            }

            // Continue with other setup
            if (_ballRenderer != null)
            {
                _propertyBlock = new MaterialPropertyBlock();
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
                   _ballView.Velocity <= _mergingProfile.MaxVelocityToMerge &&
                   otherBall.Velocity <= _mergingProfile.MaxVelocityToMerge &&
                   // Use InstanceID to ensure only one of two colliding balls initiates the merge.
                   GetInstanceID() > otherBall.GetInstanceID();
        }

        private IEnumerator EnableMergeAfterCooldown()
        {
            yield return new WaitForSeconds(_mergingProfile.MergeCooldownAfterSpawn);
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

            while (elapsedTime < _mergingProfile.MergeDuration)
            {
                // Failsafe in case the other ball is destroyed mid-animation
                if (otherBall == null) { FinalizeMerge(null, originalLayer); yield break; }

                elapsedTime += Time.deltaTime;
                float weight = Mathf.SmoothStep(0, 1, elapsedTime / _mergingProfile.MergeDuration);
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
            _onBallMerge.Raise(otherBall);

            gameObject.layer = originalLayer;
            ResetMergeShader();

            // Start the cooldown again for this newly merged ball.
            StartCoroutine(EnableMergeAfterCooldown());
        }

        // --- Shader Update Methods ---

        public void UpdateMergeShader(BallView otherBall)
        {
            if (_ballRenderer == null) return;

            float otherWorldRadius = otherBall.Radius * _mergingProfile.VisualRadiusMultiplier;
            Vector3 otherLocalPos = transform.InverseTransformPoint(otherBall.transform.position);
            Vector2 otherUV = new Vector2(
                0.5f + (otherLocalPos.x * _mergingProfile.PositionCorrectionFactor) / transform.lossyScale.x,
                0.5f + (otherLocalPos.y * _mergingProfile.PositionCorrectionFactor) / transform.lossyScale.y
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