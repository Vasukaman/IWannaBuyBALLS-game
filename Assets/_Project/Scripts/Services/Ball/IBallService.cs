// Filename: IBallService.cs
using Core.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Services.Ball
{
    public interface IBallService
    {
        /// <summary>
        /// A read-only collection of all balls currently active in the scene.
        /// </summary>
        IReadOnlyCollection<IBallView> ActiveBalls { get; }

        /// <summary>
        /// Retrieves a ball from the pool or creates a new one, then activates and configures it.
        /// </summary>
        /// <returns>The BallView component of the spawned ball.</returns>
        IBallView SpawnBall(Vector3 position, int price);

        public void InitializePool(int poolSize);
    }
}