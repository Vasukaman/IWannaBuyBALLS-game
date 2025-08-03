// Filename: BallSpawner.cs
using Gameplay.BallSystem;
using Gameplay.Interfaces;
using Reflex.Attributes;
using Services.Ball;
using Services.Registry;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A "Gadget Behaviour" that spawns a ball when activated, configured by a BallSpawnerProfile.
    /// It registers itself as an activatable target for other systems to find.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BallSpawner : MonoBehaviour, IActivatable
    {
        [Header("Configuration")]
        [Tooltip("The ScriptableObject asset that defines the behavior of this spawner.")]
        [SerializeField] private BallSpawnerProfile _profile;

        // --- Injected Dependencies ---
        [Inject] private IBallService _ballService;
        [Inject] private IActivatableRegistry _registry;

        // --- IActivatable Implementation ---
        public void Activate()
        {
            Vector3 spawnPosition = GetSpawnPosition();
            _ballService.SpawnBall(spawnPosition, 1);
        }

        public Transform ActivationTransform => this.transform;

        // --- Unity Methods ---

        private void Awake()
        {
            if (_profile == null)
            {
                Debug.LogError("BallSpawner is missing a BallSpawnerProfile! Disabling component.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _registry.Register(this);
        }

        private void OnDisable()
        {
            _registry.Unregister(this);
        }

        // --- Private Methods ---

        private Vector3 GetSpawnPosition()
        {
            // The logic now reads the configuration from the profile.
            Vector3 randomOffset = new Vector3(
                Random.Range(-_profile.SpawnRandomRange.x, _profile.SpawnRandomRange.x),
                Random.Range(-_profile.SpawnRandomRange.y, _profile.SpawnRandomRange.y),
                Random.Range(-_profile.SpawnRandomRange.z, _profile.SpawnRandomRange.z)
            );

            return transform.position + _profile.SpawnOffset + randomOffset;
        }
    }
}