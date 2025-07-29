// Filename: Ball.cs
using System;
using UnityEngine;

namespace Gameplay.BallSystem
{
    /// <summary>
    /// Represents the core data and state for a single ball.
    /// It manages its price, lifecycle events, and provides data for other components.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class Ball : MonoBehaviour
    {
        // --- Events ---
        public event Action<Ball> OnRequestDespawn;
        public event Action<int> OnPriceChanged;
        public event Action<Ball> OnDespawned;
        public event Action<Ball> OnInitialize;

        // --- Configuration ---
        [Header("Configuration")]
        [SerializeField] private int _basePrice = 10;
        [SerializeField] private Color _baseColor = Color.white;

        // --- State ---
        private int _currentPrice;
        private CircleCollider2D _collider;

        //AI is screaming at me here:(((( Yeah, I'll fix this later
        // TODO: [Circular Dependency] The Ball should not know about the factory that creates it.
        // A better approach is for the Ball to only fire an event, and the Factory listens for it.
        private IBallFactory _ballFactory;

        // --- Properties ---

        public int CurrentPrice => _currentPrice;
        public IBallFactory BallFactory => _ballFactory;
        public CircleCollider2D Collider => _collider;
        public float Radius => _collider.radius * transform.lossyScale.x;
        public Color Color { get; private set; }

        // --- Unity Methods ---

        protected virtual void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            Color = _baseColor;
            // Set initial price without invoking events on awake
            _currentPrice = _basePrice;
        }

        // --- Public API ---

        public void SetBallFactory(IBallFactory factory) => _ballFactory = factory;
        public void SetBasePrice(int newPrice) => _basePrice = newPrice;
        public void ResetToBase() => SetPrice(_basePrice);

        public void Initialize()
        {
            ResetToBase();
            OnInitialize?.Invoke(this);
            GetComponent<Rigidbody2D>().isKinematic = false;
        }

        public void SetPrice(int newPrice)
        {
            int oldPrice = _currentPrice;
            _currentPrice = Mathf.Max(1, newPrice);

            if (_currentPrice != oldPrice)
            {
                OnPriceChanged?.Invoke(_currentPrice);
            }
        }

        public void ModifyPrice(Func<int, int> modifier)
        {
            SetPrice(modifier(_currentPrice));
        }

        public void AddPrice(int amount) => ModifyPrice(old => old + amount);
        public void MultiplyPrice(float multiplier) => ModifyPrice(old => Mathf.RoundToInt(old * multiplier));
        public void SubtractPrice(int amount) => ModifyPrice(old => old - amount);

        public void Despawn()
        {
            OnDespawned?.Invoke(this);
            OnRequestDespawn?.Invoke(this);
        }

        public float GetShaderTrueSize() => Radius;
    }
}