using UnityEngine;
using Reflex.Attributes;

[RequireComponent(typeof(Collider2D))]
public class BallSpawner : MonoBehaviour, IActivatable
{
    [Header("Spawner Settings")]
    [Tooltip("Base offset from this transform at which to spawn.")]
    [SerializeField] private Vector3 spawnOffset = Vector3.zero;

    [Header("Randomness")]
    [Tooltip("Maximum random offset (X, Y, Z) added to spawn position.")]
    [SerializeField] private Vector3 spawnRandomRange = Vector3.zero;

    [Inject] private IBallFactory ballFactory;

    public void Activate()
    {
        // Compute a random offset within the specified ranges
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnRandomRange.x, spawnRandomRange.x),
            Random.Range(-spawnRandomRange.y, spawnRandomRange.y),
            Random.Range(-spawnRandomRange.z, spawnRandomRange.z)
        );

        Vector3 spawnPos = transform.position + spawnOffset + randomOffset;
        ballFactory.SpawnBall(spawnPos, 1);
    }

    // satisfy the interface
    public Transform ActivationTransform => this.transform;
}
