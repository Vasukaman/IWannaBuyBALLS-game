// Filename: BallCollisionHandler.cs
using UnityEngine;

namespace Gameplay.BallSystem
{
    /// <summary>
    /// Detects collisions with other balls and delegates the handling to the BallMerger component.
    /// This component acts as a dedicated entry point for physics collision events.
    /// </summary>
    [RequireComponent(typeof(Ball), typeof(BallMerger))]
    public class BallCollisionHandler : MonoBehaviour
    {
        private Ball _ball;
        private BallMerger _merger;

        private void Awake()
        {
            _ball = GetComponent<Ball>();
            _merger = GetComponent<BallMerger>();
        }

        /// <summary>
        /// Called by the Unity physics engine when a 2D collision occurs.
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Attempt to get a Ball component from the object we collided with.
            if (collision.collider.TryGetComponent<Ball>(out Ball otherBall))
            {
                // Ensure the other ball is not null and not this ball itself.
                if (otherBall != null && otherBall != _ball)
                {
                    // Pass the valid ball to the merger to handle the logic.
                    // In a previous refactor, we renamed TryMerge to InitiateMergeWith
                    _merger.InitiateMergeWith(otherBall);
                }
            }
        }
    }
}