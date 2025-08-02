// Filename: GameplaySceneServicesInitializer.cs
using Reflex.Attributes;
using Services.Ball;
using UnityEngine;

namespace Core
{
    /// <summary>
    /// A component that runs once when the gameplay scene loads.
    /// Its only job is to tell persistent services to initialize their scene-specific objects,
    /// like object pools.
    /// </summary>
    public class GameplaySceneServicesInitializer : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private int _initialBallPoolSize = 20;

        [Header("Scene Containers")]
        [Tooltip("An empty Transform to hold all the pooled balls.")]
        [SerializeField] private Transform _ballPoolContainer;

        [Inject] private IBallService _ballService;

        private void Start()
        {
            // Tell the BallService to create its pool, parenting the balls
            // under our scene-specific container object.
            _ballService.InitializePool(_initialBallPoolSize);
        }
    }
}