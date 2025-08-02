// Filename: BallCollisionHandler.cs
using UnityEngine;

namespace Gameplay.BallSystem
{
    /// <summary>
    /// Detects collisions with other balls and delegates the handling to the BallMerger component.
    /// This component acts as a dedicated entry point for physics collision events, keeping the
    /// collision detection logic separate from the merge execution logic.
    /// </summary>
    [RequireComponent(typeof(BallView), typeof(BallMerger))]
    public class BallCollisionHandler : MonoBehaviour
    {
        private BallView _ballView;
        private BallMerger _merger;

        private void Awake()
        {
            _ballView = GetComponent<BallView>();
            _merger = GetComponent<BallMerger>();
        }

        /// <summary>
        /// Called by the Unity physics engine when a 2D collision occurs.
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Attempt to get a BallView component from the object we collided with.
            if (collision.collider.TryGetComponent<BallView>(out BallView otherBallView))
            {
                // Ensure we are not trying to merge with ourselves.
              
                if (otherBallView != _ballView)
                {
                    // Pass the valid ball to the merger to handle the complex merge logic.
                    _merger.InitiateMergeWith(otherBallView);
                }
            }
        }
    }
}