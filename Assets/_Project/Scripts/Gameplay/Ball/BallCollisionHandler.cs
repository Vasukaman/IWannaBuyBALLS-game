// Filename: BallCollisionHandler.cs
using System; // Required for Action
using UnityEngine;

namespace Gameplay.BallSystem
{
    [RequireComponent(typeof(BallView), typeof(BallMerger))]
    public class BallCollisionHandler : MonoBehaviour
    {
        // NEW: An event to announce collisions to other components on this prefab.
        public event Action<Collision2D> OnBallCollided;

        private BallView _ballView;
        private BallMerger _merger;

        private void Awake()
        {
            _ballView = GetComponent<BallView>();
            _merger = GetComponent<BallMerger>();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Announce that a collision happened, passing along the physics data.
            OnBallCollided?.Invoke(collision);

            if (collision.collider.TryGetComponent<BallView>(out BallView otherBallView))
            {
                if (otherBallView != _ballView)
                {
                    _merger.InitiateMergeWith(otherBallView);
                }
            }
        }
    }
}