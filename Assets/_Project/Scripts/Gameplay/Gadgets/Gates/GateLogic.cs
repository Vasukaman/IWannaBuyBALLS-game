// Filename: GateLogic.cs
using Gameplay.BallSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Required for UnityEvent

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A trigger that applies a configurable effect to a Ball a single time,
    /// defined by a GateProfile. It uses UnityEvents for local feedback.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GateLogic : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GateProfile _profile;

        [Header("Local Events")]
        [Tooltip("Invoked the first time any ball passes through the gate.")]
        [SerializeField] private UnityEvent _onFirstPass; // Add this back

        [Tooltip("Invoked when a ball that has already passed through enters again.")]
        [SerializeField] private UnityEvent _onRepeatPass; // Add this back

        private readonly HashSet<BallView> _processedBalls = new HashSet<BallView>();

        private void Awake()
        {
            if (_profile == null || _profile.Effect == null)
            {
                Debug.LogError("GateLogic is missing a Profile or the Profile is missing an Effect!", this);
                enabled = false;
                return;
            }
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<BallView>(out var ball)) return;

            if (_processedBalls.Contains(ball))
            {
                // Invoke the repeat pass event
                _onRepeatPass?.Invoke();
                return;
            }

            _profile.Effect.Apply(ball.Data);

            _processedBalls.Add(ball);
            ball.OnDespawned += HandleBallDespawned;

            // Invoke the first pass event
            _onFirstPass?.Invoke();
        }

        private void OnDestroy()
        {
            foreach (var ball in _processedBalls)
            {
                if (ball != null)
                {
                    ball.OnDespawned -= HandleBallDespawned;
                }
            }
        }

        private void HandleBallDespawned(BallView ball)
        {
            if (_processedBalls.Remove(ball))
            {
                ball.OnDespawned -= HandleBallDespawned;
            }
        }
    }
}