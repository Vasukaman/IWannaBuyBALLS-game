// Filename: BallSpawnerProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBallSpawnerProfile", menuName = "Profiles/Gadgets/Ball Spawner Profile")]
public class BallSpawnerProfile : ScriptableObject
{
    [Header("Spawner Settings")]
    [Tooltip("Base offset from the spawner's transform at which to spawn.")]
    public Vector3 SpawnOffset = Vector3.zero;

    [Header("Randomness")]
    [Tooltip("Maximum random offset applied to the spawn position on each axis.")]
    public Vector3 SpawnRandomRange = Vector3.zero;
}