// Filename: BallService.cs
using Core.Spawning;
using Gameplay.BallSystem;
using Reflex.Core;
using Reflex.Injectors;
using System.Collections.Generic;
using UnityEngine;

namespace Services.Ball
{
    public class BallService : IBallService
    {
        public IReadOnlyCollection<BallView> ActiveBalls => _activeBalls;

        private readonly IPrefabInstantiator _instantiator;
        private readonly Container _container;
        private readonly BallView _ballPrefab; // The service knows the blueprint

        private readonly Queue<BallView> _pooledBalls = new Queue<BallView>();
        private readonly HashSet<BallView> _activeBalls = new HashSet<BallView>();

        // All dependencies are passed in by the DI container when this service is created.
        public BallService(IPrefabInstantiator instantiator, Container container, BallView ballPrefab, int initialPoolSize)
        {
            _instantiator = instantiator;
            _container = container;
            _ballPrefab = ballPrefab;

            PopulateInitialPool(initialPoolSize);
        }

        public BallView SpawnBall(Vector3 position, int price)
        {
            Debug.Log("Service Creating Ball");
            BallView ball = GetOrCreateBall();
            ConfigureBall(ball, position, price);
            _activeBalls.Add(ball);
            return ball;
        }

        private void PopulateInitialPool(int poolSize)
        {
            for (int i = 0; i < poolSize; i++)
            {
                BallView ball = CreateNewBallInstance();
                ball.gameObject.SetActive(false);
                _pooledBalls.Enqueue(ball);
            }
        }

        private BallView GetOrCreateBall()
        {
            if (_pooledBalls.Count > 0)
            {
                return _pooledBalls.Dequeue();
            }
            return CreateNewBallInstance();
        }

        private BallView CreateNewBallInstance()
        {
            // The service tells its "worker" to create the object.
            GameObject instance = _instantiator.InstantiatePrefab(_ballPrefab.gameObject, Vector3.zero);

            // Then it tells the container to inject dependencies.
            GameObjectInjector.InjectObject(instance, Container.ProjectContainer);

            return instance.GetComponent<BallView>();
        }

        private void ConfigureBall(BallView ball, Vector3 position, int price)
        {
            ball.transform.position = position;
            ball.SetBasePrice(price);

            // The service subscribes to the ball's event to handle pooling.
            ball.OnRequestDespawn += ReturnBallToPool;

            ball.gameObject.SetActive(true);
            ball.Initialize();
        }

        private void ReturnBallToPool(BallView ball)
        {
            ball.OnRequestDespawn -= ReturnBallToPool;
            _activeBalls.Remove(ball);
            ball.gameObject.SetActive(false);
            _pooledBalls.Enqueue(ball);
        }
    }
}