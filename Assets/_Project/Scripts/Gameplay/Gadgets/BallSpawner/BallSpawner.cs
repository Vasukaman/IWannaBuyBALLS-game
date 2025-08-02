// Filename: BallSpawner.cs
using Gameplay.BallSystem;
using Reflex.Attributes;
using Services.Ball;
using Services.Registry;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A "Gadget Behaviour" that spawns a ball when activated. It registers itself
    /// as an activatable target for other systems to find.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BallSpawner : MonoBehaviour, IActivatable
    {
        [Header("Spawner Settings")]
        [Tooltip("Base offset from this transform's position at which to spawn.")]
        [SerializeField] private Vector3 _spawnOffset = Vector3.zero;

        [Header("Randomness")]
        [Tooltip("Maximum random offset applied to the spawn position on each axis.")]
        [SerializeField] private Vector3 _spawnRandomRange = Vector3.zero;

        // --- Injected Dependencies ---
        [Inject] private IBallService _ballService;
        [Inject] private IActivatableRegistry _registry;

        // --- IActivatable Implementation ---

        /// <summary>
        /// Spawns a new ball by calling the central BallService.
        /// </summary>
        public void Activate()
        {
            Vector3 spawnPosition = GetSpawnPosition();
            _ballService.SpawnBall(spawnPosition, 1);
        }

        /// <summary>
        /// Provides this spawner's transform as the target for activators.
        /// </summary>
        public Transform ActivationTransform => this.transform;

        // --- Registry Management ---

        private void OnEnable()
        {
            // When this spawner becomes active, it registers itself in the global "phone book"
            // of activatable objects, so other systems (like AutoActivator) can find it.
            _registry.Register(this);
        }

        private void OnDisable()
        {
            // It's crucial to unregister when disabled or destroyed to prevent errors.
            _registry.Unregister(this);
        }

        // --- Private Methods ---

        /// <summary>
        /// Calculates the final world-space position for the new ball.
        /// </summary>
        private Vector3 GetSpawnPosition()
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-_spawnRandomRange.x, _spawnRandomRange.x),
                Random.Range(-_spawnRandomRange.y, _spawnRandomRange.y),
                Random.Range(-_spawnRandomRange.z, _spawnRandomRange.z)
            );

            return transform.position + _spawnOffset + randomOffset;
        }
    }
}