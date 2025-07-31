// Filename: BallView.cs (replaces Ball.cs)
using System;
using UnityEngine;

namespace Gameplay.BallSystem
{
    /// <summary>
    /// The MonoBehaviour component for a Ball (the "Body"). It acts as the "bridge" between the
    /// pure C# BallData and the Unity engine (physics, visuals, lifecycle events).
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class BallView : MonoBehaviour
    {
        // --- Events ---
        // These events are for other MonoBehaviours to listen to.
        public event Action<BallView> OnRequestDespawn;
        public event Action<BallView> OnDespawned;
        public event Action<BallView> OnInitialize;

        // --- Configuration ---
        [Header("Configuration")]
        [SerializeField] private int _basePrice = 1;
        [SerializeField] private Color _baseColor = Color.white;

        // --- Properties ---

        /// <summary>
        /// The "Brain" of the ball, containing its data and non-Unity logic.
        /// </summary>
        public BallData Data { get; private set; }

        public CircleCollider2D Collider { get; private set; }
        public float Radius => Collider.radius * transform.lossyScale.x;
        public Color Color { get; private set; }

        // --- Unity Methods ---
        private void Awake()
        {
            Collider = GetComponent<CircleCollider2D>();
            Color = _baseColor;

            // The View creates its own Brain upon waking up.
            Data = new BallData(_basePrice);
        }

        // --- Public API ---

        /// <summary>
        /// Used by the factory to configure the ball's base price upon spawning.
        /// </summary>
        public void SetBasePrice(int newBasePrice)
        {
            // The View tells its Brain to update the base price.
            Data = new BallData(newBasePrice);
        }

        /// <summary>
        /// Prepares the ball for gameplay, resetting its state and activating physics.
        /// </summary>
        public void Initialize()
        {
            Data.ResetToBase();
            GetComponent<Rigidbody2D>().isKinematic = false;
            OnInitialize?.Invoke(this);
        }

        /// <summary>
        /// Signals that this ball should be returned to the pool.
        /// The BallFactory will listen for the OnRequestDespawn event.
        /// </summary>
        public void Despawn()
        {
            OnDespawned?.Invoke(this);
            OnRequestDespawn?.Invoke(this);
        }
    }
}