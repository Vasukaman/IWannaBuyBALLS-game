// BallCollisionHandler.cs - Handles collision detection
using UnityEngine;

[RequireComponent(typeof(Ball), typeof(BallMerger))]
public class BallCollisionHandler : MonoBehaviour
{
    private Ball _ball;
    private BallMerger _merger;

    private void Awake()
    {
        _ball = GetComponent<Ball>();
        _merger = GetComponent<BallMerger>();
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Ball other = col.collider.GetComponent<Ball>();
        if (other == null || other == _ball) return;

        _merger.TryMerge(other);
    }
}