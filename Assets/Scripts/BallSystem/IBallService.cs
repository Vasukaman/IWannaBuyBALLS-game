// Filename: IBallService.cs
using Gameplay.BallSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Services.Ball
{
    public interface IBallService
    {
        /// <summary>
        /// A read-only collection of all balls currently active in the scene.
        /// </summary>
        IReadOnlyCollection<BallView> ActiveBalls { get; }

        /// <summary>
        /// Retrieves a ball from the pool or creates a new one, then activates and configures it.
        /// </summary>
        /// <returns>The BallView component of the spawned ball.</returns>
        BallView SpawnBall(Vector3 position, int price);
    }
}