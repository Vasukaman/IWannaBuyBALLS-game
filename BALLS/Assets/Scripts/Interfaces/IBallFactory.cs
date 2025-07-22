using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBallFactory
{
    /// <summary>
    /// Spawns a ball at the given world position, with the given base price.
    /// </summary>
    Ball SpawnBall(Vector3 position, int basePrice);
    public IReadOnlyCollection<Ball> GetActiveBalls();
}
