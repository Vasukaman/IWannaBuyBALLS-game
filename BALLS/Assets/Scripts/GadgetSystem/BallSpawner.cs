// Filename: BallSpawner.cs
using Gameplay.BallSystem;
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A gadget that spawns a ball when activated.
    /// It uses a BallFactory to create the ball instance.
    /// </summary>

    public class BallSpawner : MonoBehaviour, IActivatable
    {
        [Header("Spawner Settings")]
        [Tooltip("Base offset from this transform's position at which to spawn.")]
        [SerializeField] private Vector3 _spawnOffset = Vector3.zero;

        [Header("Randomness")]
        [Tooltip("Maximum random offset applied to the spawn position on each axis.")]
        [SerializeField] private Vector3 _spawnRandomRange = Vector3.zero;
        [Inject] private IBallFactory _ballFactory;

        // --- IActivatable Implementation ---

        /// <summary>
        /// Spawns a new ball at a calculated position.
        /// </summary>
        public void Activate()
        {
            Vector3 spawnPosition = GetSpawnPosition();
            _ballFactory.SpawnBall(spawnPosition, 1);
        }

        public Transform ActivationTransform => this.transform;

        // --- Private Methods ---

        /// <summary>
        /// Calculates the final world-space position for the new ball, including offsets and randomness.
        /// </summary>
        /// <returns>The calculated world-space spawn position.</returns>
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