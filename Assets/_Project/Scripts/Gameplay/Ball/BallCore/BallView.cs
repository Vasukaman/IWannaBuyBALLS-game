// Filename: BallView.cs (replaces Ball.cs)
using Core;
using Reflex.Attributes;
using Services.Money;
using System;
using UnityEngine;
using VFX.BallEffects;

namespace Gameplay.BallSystem
{
    /// <summary>
    /// The MonoBehaviour component for a Ball (the "Body"). It acts as the "bridge" between the
    /// pure C# BallData and the Unity engine (physics, visuals, lifecycle events).
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class BallView : MonoBehaviour
    {
        // [Inject] IMoneyService money;
        // --- Events ---
        // These events are for other MonoBehaviours to listen to.
        public event Action<BallView> OnRequestDespawn;
        public event Action<BallView> OnDespawned;
        public event Action<BallView> OnInitialize;
        public bool CanMerge { get; set; } = false;

        // --- Configuration ---
        [Header("Configuration")]
        [Tooltip("The master profile that defines all aspects of this ball.")]
        [SerializeField] private BallProfile _profile;

        public BallProfile Profile => _profile;
        public BallData Data { get; private set; }
        public Color Color { get; private set; }


        // --- Properties ---

        /// <summary>
        /// The "Brain" of the ball, containing its data and non-Unity logic.
        /// </summary>
        /// 

        public CircleCollider2D Collider { get; private set; }

        public float Radius => Collider.radius * transform.lossyScale.x;

        private Rigidbody2D _rigidbody;
        public float Velocity => _rigidbody.velocity.magnitude;

        // --- Unity Methods ---
        private void Awake()
        {
            Collider = GetComponent<CircleCollider2D>();
            _rigidbody = GetComponent<Rigidbody2D>();

            // The View now gets its color from the central data profile.
            if (_profile != null && _profile.Appearance != null)
            {
                Color = _profile.Appearance.BaseColor;
            }
            else
            {
                Debug.LogError("BallProfile or its AppearanceProfile is not assigned!", this);
            }

            // The View creates its own Brain.
            Data = new BallData(_profile != null ? _profile.BasePrice : 1); // Use a base price from the profile
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
           // money.Add(123);
            Data.ResetToBase();
            _rigidbody.isKinematic = false;
            OnInitialize?.Invoke(this);
            gameObject.layer = GameLayers.Ball;
        }


        /// <summary>
        /// Disables physics components so the ball can be controlled by an animation.
        /// </summary>
        public void PrepareForSellingAnimation()
        {

            if (_rigidbody == null) return;
                _rigidbody.isKinematic = true;
                _rigidbody.velocity = Vector2.zero;
                _rigidbody.angularVelocity = 0;
            
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

        public float GetShaderTrueSize() => Radius;
    }




}