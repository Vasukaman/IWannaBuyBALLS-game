// Filename: GateLogic.cs
using Gameplay.BallSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay.Gadgets
{
    [RequireComponent(typeof(Collider2D))]
    public class GateLogic : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private GateProfile _profile;



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
   
                return;
            }

            _profile.Effect.Apply(ball.Data, _profile.Ammount);

            _processedBalls.Add(ball);
            ball.OnDespawned += HandleBallDespawned;
           
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
        /// Called when a ball we've processed is despawned. This allows it to be processed again
        /// if it is respawned from a pool and re-enters the gate.
        /// </summary>
        private void HandleBallDespawned(BallView ball)
        {
            // Remove the ball from the processed set and unsubscribe from its event.
            if (_processedBalls.Remove(ball))
            {
                ball.OnDespawned -= HandleBallDespawned;
            }
        }
    }
}