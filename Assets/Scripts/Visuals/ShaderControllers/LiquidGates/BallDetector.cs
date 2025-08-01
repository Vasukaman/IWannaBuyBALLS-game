// Filename: BallDetector.cs
using Gameplay.BallSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Physics
{
    /// <summary>
    /// A reusable component that detects all BallView instances within its trigger.
    /// It uses non-allocating physics queries for high performance.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BallDetector : MonoBehaviour
    {
        public IReadOnlyList<BallView> DetectedBalls => _detectedBalls;

        private readonly List<BallView> _detectedBalls = new List<BallView>();
        private Collider2D _triggerCollider;
        private ContactFilter2D _contactFilter;
        private readonly List<Collider2D> _results = new List<Collider2D>();

        private void Awake()
        {
            _triggerCollider = GetComponent<Collider2D>();
            _triggerCollider.isTrigger = true;
            _contactFilter.useTriggers = true;
        }

        private void FixedUpdate()
        {
            _triggerCollider.OverlapCollider(_contactFilter, _results);
            _detectedBalls.Clear();

            foreach (var col in _results)
            {
                if (col.TryGetComponent<BallView>(out var ball))
                {
                    _detectedBalls.Add(ball);
                }
            }
        }
    }
}