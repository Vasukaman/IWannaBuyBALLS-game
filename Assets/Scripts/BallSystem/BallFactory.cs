// Filename: BallFactory.cs
using Gameplay.BallSystem;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.BallSystem
{
    // TODO: [Architecture] Making a factory a MonoBehaviour can be inflexible. For larger projects,
    // a plain C# factory class managed by a Dependency Injection container is often a better pattern,
    // as it separates the pure logic of object creation from the Unity scene hierarchy.

    //Ok, this feels important to know how to do, reasearching and changing SOON
    public class BallFactory : MonoBehaviour, IBallFactory
    {
        [Header("Configuration")]
        [SerializeField] private Ball _ballPrefab;
        [SerializeField] private int _initialPoolSize = 20;

        // --- State ---
        private readonly Queue<Ball> _pooledBalls = new Queue<Ball>();
        private readonly HashSet<Ball> _activeBalls = new HashSet<Ball>();

        // --- Properties ---

        /// <summary>
        /// A read-only collection of all balls currently active in the scene.
        /// </summary>
        public IReadOnlyCollection<Ball> ActiveBalls => _activeBalls;

        // --- Unity Methods ---

        private void Start()
        {
            PopulateInitialPool();
        }

        private void OnDestroy()
        {
            // Clean up event subscriptions for any remaining active balls to prevent memory leaks.
            foreach (var ball in _activeBalls)
            {
                if (ball != null)
                {
                    ball.OnRequestDespawn -= ReturnBallToPool;
                }
            }
            _activeBalls.Clear();
        }

        // --- Public API ---

        // TODO: [Redundancy] This method duplicates the functionality of the 'ActiveBalls' public property.
        // It can be removed if all other scripts are updated to use the property instead.
        public IReadOnlyCollection<Ball> GetActiveBalls()
        {
            return _activeBalls;
        }

        /// <summary>
        /// Retrieves a ball from the pool or creates a new one, then activates and configures it.
        /// </summary>
        public Ball SpawnBall(Vector3 position, int price)
        {
            Ball ball = GetOrCreateBall();
            ConfigureBall(ball, position, price);

            _activeBalls.Add(ball);
            return ball;
        }

        // --- Private Methods ---

        /// <summary>
        /// Fills the object pool with a pre-defined number of inactive ball instances.
        /// </summary>
        private void PopulateInitialPool()
        {
            for (int i = 0; i < _initialPoolSize; i++)
            {
                Ball ball = Instantiate(_ballPrefab, transform);
                ball.gameObject.SetActive(false);
                _pooledBalls.Enqueue(ball);
            }
        }

        /// <summary>
        /// Gets an inactive ball from the pool or instantiates a new one if the pool is empty.
        /// </summary>
        private Ball GetOrCreateBall()
        {
            if (_pooledBalls.Count > 0)
            {
                return _pooledBalls.Dequeue();
            }

            // The pool is empty, so we create a new instance as a fallback.
            return Instantiate(_ballPrefab, transform);
        }

        /// <summary>
        /// Sets up a ball's properties and state before it becomes active in the scene.
        /// </summary>
        private void ConfigureBall(Ball ball, Vector3 position, int price)
        {
            ball.transform.position = position;
            ball.SetBasePrice(price);

            // TODO: [Circular Dependency] The factory injects a reference to itself into the ball.
            // This creates a tight coupling between the creator and the product. A better pattern is
            // for the factory to subscribe to the ball's 'OnRequestDespawn' event, and the ball
            // would have no knowledge of the factory.
            ball.SetBallFactory(this);
            ball.OnRequestDespawn += ReturnBallToPool;

            ball.gameObject.SetActive(true);
            ball.Initialize(); // This should be the last step to ensure all setup is done before events fire.
        }

        /// <summary>
        /// The event handler called by a ball when it needs to be despawned.
        /// </summary>
        private void ReturnBallToPool(Ball ball)
        {
            // Unsubscribe to prevent this being called again for the same instance.
            ball.OnRequestDespawn -= ReturnBallToPool;

            _activeBalls.Remove(ball);

            ball.gameObject.SetActive(false);
            _pooledBalls.Enqueue(ball);
        }
    }
}