using System.Collections.Generic;
using UnityEngine;

public class BallFactory : MonoBehaviour, IBallFactory
{
    private readonly HashSet<Ball> _activeBalls = new HashSet<Ball>();
    public IReadOnlyCollection<Ball> ActiveBalls => _activeBalls;

    [SerializeField] private Ball ballPrefab;
    [SerializeField] private int initialPoolSize = 20;

    private readonly Queue<Ball> _pool = new Queue<Ball>();

    private void Start() => PreparePool();

    public IReadOnlyCollection<Ball> GetActiveBalls()
    {
        return _activeBalls;
    }
    private void PreparePool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            var ball = Instantiate(ballPrefab, transform);
            ball.gameObject.SetActive(false);
            _pool.Enqueue(ball);
        }
    }

    public Ball SpawnBall(Vector3 position, int price)
    {
        Ball ball = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(ballPrefab, transform);

        ball.transform.position = position;
        ball.SetBasePrice(price);
        ball.ResetToBase();
        ball.OnRequestDespawn += ReturnToPool;
        ball.SetBallFactory(this);
        ball.gameObject.SetActive(true);
        ball.Initialize();
        _activeBalls.Add(ball);

       // Debug.Log("Ball Spawned at " + ball.transform.position.ToString() + " with scale " + ball.transform.lossyScale.ToString());

        return ball;
    }

    private void ReturnToPool(Ball ball)
    {
        ball.OnRequestDespawn -= ReturnToPool;
        ball.gameObject.SetActive(false);
        _pool.Enqueue(ball);
        _activeBalls.Remove(ball);
    }
}