// Filename: BallSellerZone.cs
using Game.Economy;
using Gameplay.BallSystem;
using Reflex.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A trigger zone that "sells" any Ball entering it. It plays an animation,
    /// adds the ball's price to the player's balance, and despawns the ball.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BallSeller : MonoBehaviour
    {
        [Header("Animation")]
        [Tooltip("How long the selling animation takes in seconds.")]
        [SerializeField] private float _animationDuration = 0.3f;

        [Header("Configuration")]
        [Tooltip("If true, the ball's collider will be disabled during the animation.")]
        [SerializeField] private bool _disableCollider = false;

        // --- Dependencies ---
        [Inject] private IMoneyService _moneyService;

        // --- State ---
        private readonly HashSet<Ball> _ballsBeingProcessed = new();

        // --- Unity Methods ---

        private void Awake()
        {
            // Ensure the collider is a trigger to receive OnTriggerEnter2D events.
            if (TryGetComponent<Collider2D>(out var col))
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<Ball>(out var ball))
            {
                // Start the selling process only if we aren't already handling this ball.
                // This prevents bugs from multiple trigger events for the same object.
                if (_ballsBeingProcessed.Add(ball))
                {
                    StartCoroutine(AnimateAndSellBall(ball));
                }
            }
        }

        // --- Private Logic ---

        /// <summary>
        /// Handles the entire lifecycle of selling a ball, from animation to despawn.
        /// </summary>
        private IEnumerator AnimateAndSellBall(Ball ball)
        {
            // Prepare the ball for animation by disabling its physics interactions.
            PrepareBallForAnimation(ball);

            Vector3 startPosition = ball.transform.position;
            Vector3 startScale = ball.transform.localScale;
            float elapsedTime = 0f;

            // Animate the ball moving towards the center of the seller and shrinking.
            while (elapsedTime < _animationDuration)
            {
                // Ensure the ball still exists before trying to access its transform.
                if (ball == null) yield break;

                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / _animationDuration);

                ball.transform.position = Vector3.Lerp(startPosition, transform.position, progress);
                ball.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);

                yield return null;
            }

            // Finalize the transaction.
            if (ball != null)
            {
                _moneyService.Add(ball.CurrentPrice);
                ball.Despawn();
            }

            // Clean up the tracking set.
            _ballsBeingProcessed.Remove(ball);
        }

        /// <summary>
        /// Disables physics components on the ball so it can be animated freely.
        /// </summary>
        private void PrepareBallForAnimation(Ball ball)
        {
            // TODO: [Architecture] This component directly manipulates the ball's state (disabling physics,
            // changing transform). A more decoupled approach would be for the ball to have its own
            // "AnimateToAndDespawn" method that this component could call, making the ball
            // responsible for its own animation and state changes.
            if (_disableCollider && ball.TryGetComponent<Collider2D>(out var ballCollider))
            {
                ballCollider.enabled = false;
            }

            if (ball.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.isKinematic = true;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0;
            }
        }
    }
}