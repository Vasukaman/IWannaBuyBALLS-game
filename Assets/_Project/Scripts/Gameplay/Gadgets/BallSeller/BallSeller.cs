// Filename: BallSellerZone.cs
using Core.Events;
using Cysharp.Threading.Tasks;
using Gameplay.BallSystem;
using Reflex.Attributes;
using Services.Money;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A trigger zone that "sells" any Ball entering it. It plays an animation,
    /// adds the ball's value to the player's balance, and despawns the ball.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BallSeller : MonoBehaviour
    {

        [Header("Event Channel")]
        [Tooltip("The event to raise when a ball is successfully sold.")]
        [SerializeField] private IntGameEvent _onBallSold;

        [Header("Animation")]
        [Tooltip("How long the selling animation takes in seconds.")]
        [SerializeField] private float _animationDuration = 0.3f;

        // --- Injected Dependencies ---
        [Inject] private IMoneyService _moneyService;

        // --- State ---
        private readonly HashSet<BallView> _ballsBeingProcessed = new();



        // --- Unity Methods ---

        private void Awake()
        {
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<BallView>(out var ball))
            {
                // The HashSet.Add method returns true if the item was successfully added,
                // which means it wasn't already in the set. This is a clean way to
                // ensure we only process each ball once.
                if (_ballsBeingProcessed.Add(ball))
                {
                    AnimateAndSellBall(ball).Forget();
                }
            }
        }

        // --- Private Logic ---

        /// <summary>
        /// Handles the entire lifecycle of selling a ball, from animation to despawn.
        /// </summary>
        private async UniTaskVoid AnimateAndSellBall(BallView ball)
        {
            // 1. Tell the ball to prepare itself for the animation.
            ball.PrepareForSellingAnimation();

            Vector3 startPosition = ball.transform.position;
            Vector3 startScale = ball.transform.localScale;
            float elapsedTime = 0f;

            var cancellationToken = this.GetCancellationTokenOnDestroy();
            // 2. Animate the ball.
            while (elapsedTime < _animationDuration)
            {
                // A null check is crucial inside a coroutine, as the object
                // could be destroyed by another process while the animation is running.
                if (ball == null)
                {
                    // If the ball is gone, we must also clean up our tracking set.
                    _ballsBeingProcessed.Remove(ball);
                    return;
                }

                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / _animationDuration);

                ball.transform.position = Vector3.Lerp(startPosition, transform.position, progress);
                ball.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

                await UniTask.Yield(cancellationToken);
            }

            // 3. Finalize the transaction.
            if (ball != null)
            {
                _onBallSold.Raise(ball.Data.CurrentPrice);
                ball.Despawn();
            }

            // 4. Clean up the tracking set.
            // This is safe to call even if the ball was already removed (e.g., if it was destroyed mid-animation).
            _ballsBeingProcessed.Remove(ball);
        }
    }
}