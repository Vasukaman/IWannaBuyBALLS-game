// Filename: BallMerger.cs
using Core;
using Core.Events;
using Gameplay.BallSystem;
using Reflex.Attributes;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

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
            _ballView = GetComponent<BallView>();

            if (_ballView.Profile != null)
            {
                _mergingProfile = _ballView.Profile.Merging;
            }

            if (_mergingProfile == null)
            {
                Debug.LogError("BallMergingProfile is not assigned! Disabling BallMerger.", this);
                enabled = false;
                return;
            }

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

            _ballView.CanMerge = false;
            otherBall.CanMerge = false;

            // Call the async UniTask method directly. The .Forget() call indicates
            // this is a "fire and forget" task.
            MergeTask(otherBall).Forget();
        }

        // --- Event Handlers ---

        private void HandleBallInitialized(BallView ball)
        {
            _ballView.CanMerge = false;
            // Call the async UniTask method instead of starting a coroutine.
            EnableMergeAfterCooldown().Forget();
        }

        private void HandleBallDespawned(BallView ball)
        {
            ResetMergeShader();
        }

        // --- Private Logic & Async Tasks ---

        private bool CanMergeWith(BallView otherBall)
        {
            return _ballView.CanMerge &&
                   otherBall.CanMerge &&
                   _ballView.Data.CurrentPrice == otherBall.Data.CurrentPrice &&
                   _ballView.Velocity <= _mergingProfile.MaxVelocityToMerge &&
                   otherBall.Velocity <= _mergingProfile.MaxVelocityToMerge &&
                   GetInstanceID() > otherBall.GetInstanceID();
        }

        /// <summary>
        /// Replaces the EnableMergeAfterCooldown coroutine.
        /// </summary>
        private async UniTaskVoid EnableMergeAfterCooldown()
        {
            // UniTask.Delay is a zero-allocation, more precise replacement for WaitForSeconds.
            // It also automatically handles cancellation if the object is destroyed.
            await UniTask.Delay(TimeSpan.FromSeconds(_mergingProfile.MergeCooldownAfterSpawn), cancellationToken: this.GetCancellationTokenOnDestroy());

            // The failsafe "if (this != null)" is no longer needed because the CancellationToken handles it.
            _ballView.CanMerge = true;
        }

        /// <summary>
        /// Replaces the MergeCoroutine with a much cleaner async UniTask method.
        /// </summary>
        private async UniTaskVoid MergeTask(BallView otherBall)
        {
            var originalLayer = gameObject.layer;
            gameObject.layer = GameLayers.MergingBall;
            otherBall.gameObject.layer = GameLayers.MergingBall;

            var otherMerger = otherBall.GetComponent<BallMerger>();
            if (otherMerger == null)
            {
                FinalizeMerge(otherBall, originalLayer);
                return;
            }

            float elapsedTime = 0f;
            Vector3 thisStartPosition = transform.position;
            Vector3 otherStartPosition = otherBall.transform.position;

            var cancellationToken = this.GetCancellationTokenOnDestroy();

            while (elapsedTime < _mergingProfile.MergeDuration)
            {
                // Failsafe if the other ball is destroyed mid-animation
                if (otherBall == null)
                {
                    FinalizeMerge(null, originalLayer);
                    return;
                }

                elapsedTime += Time.deltaTime;
                float weight = Mathf.SmoothStep(0, 1, elapsedTime / _mergingProfile.MergeDuration);
                Vector3 mergeCenter = Vector3.Lerp(thisStartPosition, otherStartPosition, 0.5f);

                transform.position = Vector3.Lerp(thisStartPosition, mergeCenter, weight);
                otherBall.transform.position = Vector3.Lerp(otherStartPosition, mergeCenter, weight);

                UpdateMergeShader(otherBall);
                otherMerger.UpdateMergeShader(_ballView);

             //   [cite_start]// This replaces "yield return null" and automatically cancels if the GameObject is destroyed. [cite: 1]
                await UniTask.Yield(cancellationToken);
            }

            FinalizeMerge(otherBall, originalLayer);
        }

        private void FinalizeMerge(BallView otherBall, int originalLayer)
        {
            if (otherBall != null)
            {
                _ballView.Data.MultiplyPrice(2f);
                otherBall.Despawn();
            }
            _onBallMerge.Raise(otherBall);

            gameObject.layer = originalLayer;
            ResetMergeShader();

            // Start the cooldown again for this newly merged ball.
            EnableMergeAfterCooldown().Forget();
        }

        // --- Shader Update Methods ---

        public void UpdateMergeShader(BallView otherBall)
        {
            if (_ballRenderer == null || otherBall == null) return;

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