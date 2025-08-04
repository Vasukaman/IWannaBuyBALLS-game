// Filename: BallService.cs
using Core.Interfaces;
using Core.Spawning;

using Reflex.Core;
using Reflex.Injectors;
using System.Collections.Generic;
using UnityEngine;

namespace Services.Ball
{
    public class BallService : IBallService
    {
        public IReadOnlyCollection<IBallView> ActiveBalls => _activeBalls;

        private readonly IPrefabInstantiator _instantiator;
        private readonly Container _container;
        private readonly GameObject _ballPrefabBlueprint; // The service knows the blueprint

        private readonly Queue<IBallView> _pooledBalls = new Queue<IBallView>();
        private readonly HashSet<IBallView> _activeBalls = new HashSet<IBallView>();

        // All dependencies are passed in by the DI container when this service is created.
        public BallService(IPrefabInstantiator instantiator, Container container, GameObject ballPrefabBlueprint, int initialPoolSize)
        {
            _instantiator = instantiator;
            _container = container;
            _ballPrefabBlueprint = ballPrefabBlueprint;

        }

        public IBallView SpawnBall(Vector3 position, int price)
        {
            IBallView ball = GetOrCreateBall();
            ConfigureBall(ball, position, price);
            _activeBalls.Add(ball);
            return ball;
        }
        public void InitializePool(int poolSize)
        {
            PopulateInitialPool(poolSize);
        }

        private void PopulateInitialPool(int poolSize)
        {
            for (int i = 0; i < poolSize; i++)
            {
                IBallView ball = CreateNewBallInstance();
                ball.gameObject.SetActive(false);
                _pooledBalls.Enqueue(ball);
            }
        }

        private IBallView GetOrCreateBall()
        {
            if (_pooledBalls.Count > 0)
            {
                return _pooledBalls.Dequeue();
            }
            return CreateNewBallInstance();
        }


        private IBallView CreateNewBallInstance(Transform parent = null)
        {
            // 1. Instantiate the generic blueprint.
            GameObject instance = _instantiator.InstantiatePrefab(_ballPrefabBlueprint, Vector3.zero, parent);

            // 2. Perform a safe, runtime check to get the specific component.
            if (!instance.TryGetComponent<IBallView>(out var ballView))
            {
                Debug.LogError($"The assigned Ball Prefab '{_ballPrefabBlueprint.name}' is missing a component that implements IBallView!", instance);
                Object.Destroy(instance);
                return null;
            }

            // 3. Inject dependencies.
            GameObjectInjector.InjectObject(instance, _container);
            return ballView;
        }

        private void ConfigureBall(IBallView ball, Vector3 position, int price)
        {
            ball.transform.position = position;
            ball.SetBasePrice(price);

            // The service subscribes to the ball's event to handle pooling.
            ball.OnRequestDespawn += ReturnBallToPool;

            ball.gameObject.SetActive(true);
            ball.Initialize();
        }

        private void ReturnBallToPool(IBallView ball)
        {
            ball.OnRequestDespawn -= ReturnBallToPool;
            _activeBalls.Remove(ball);
            ball.gameObject.SetActive(false);
            _pooledBalls.Enqueue(ball);
        }
    }
}