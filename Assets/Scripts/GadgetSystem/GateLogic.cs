// Filename: GateLogic.cs
using Gameplay.BallSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A trigger that modifies the price of a Ball a single time as it passes through.
    /// It uses UnityEvents to allow for additional custom responses.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GateLogic : MonoBehaviour
    {
        public enum GateOperation
        {
            Add,
            Multiply,
            Subtract
        }

        [Header("Gate Configuration")]
        [SerializeField] private GateOperation _gateOperation;
        [SerializeField] private float _modifierValue = 1;

        [Header("Events")]
        [Tooltip("Invoked the first time any ball passes through the gate.")]
        [SerializeField] private UnityEvent<Ball> _onFirstPass;
        [Tooltip("Invoked when a ball that has already passed through enters the trigger again.")]
        [SerializeField] private UnityEvent<Ball> _onRepeatPass;

        // --- State ---
        private readonly HashSet<Ball> _processedBalls = new HashSet<Ball>();

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
            if (!other.TryGetComponent<Ball>(out var ball))
            {
                return;
            }

            // If we have already processed this ball, invoke the repeat pass event and do nothing else.
            if (_processedBalls.Contains(ball))
            {
                _onRepeatPass?.Invoke(ball);
                return;
            }

            // Apply the primary effect, add the ball to our set, and listen for its despawn event.
            ApplyGateEffect(ball);
            _processedBalls.Add(ball);
            ball.OnDespawned += HandleBallDespawned;
            _onFirstPass?.Invoke(ball);
        }

        private void OnDestroy()
        {
            // When this gate is destroyed, clean up all remaining event subscriptions to prevent memory leaks.
            foreach (var ball in _processedBalls)
            {
                if (ball != null)
                {
                    ball.OnDespawned -= HandleBallDespawned;
                }
            }
        }

        // --- Private Methods ---

        /// <summary>
        /// Modifies the ball's price based on the gate's configured operation and value.
        /// </summary>
        private void ApplyGateEffect(Ball ball)
        {
            switch (_gateOperation)
            {
                case GateOperation.Add:
                    ball.AddPrice(Mathf.RoundToInt(_modifierValue));
                    break;
                case GateOperation.Multiply:
                    ball.MultiplyPrice(_modifierValue);
                    break;
                case GateOperation.Subtract:
                    ball.SubtractPrice(Mathf.RoundToInt(_modifierValue));
                    break;
            }
        }

        /// <summary>
        /// Called when a ball we've processed is despawned. This allows it to be processed again
        /// if it is respawned from a pool and re-enters the gate.
        /// </summary>
        private void HandleBallDespawned(Ball ball)
        {
            // Remove the ball from the processed set and unsubscribe from its event.
            if (_processedBalls.Remove(ball))
            {
                ball.OnDespawned -= HandleBallDespawned;
            }
        }
    }
}