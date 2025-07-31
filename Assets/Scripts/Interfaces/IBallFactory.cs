using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.BallSystem;
public interface IBallFactory
{
    /// <summary>
    /// Spawns a ball at the given world position, with the given base price.
    /// </summary>
    BallView SpawnBall(Vector3 position, int basePrice);
    public IReadOnlyCollection<BallView> GetActiveBalls();
}
