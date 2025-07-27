using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum GateType
{
    Add,
    Multiply,
    Subtract
}

[RequireComponent(typeof(Collider2D))]
public class GateLogic : MonoBehaviour
{
    [SerializeField] private UnityEvent<Ball> onFirstPass;
    [SerializeField] private UnityEvent<Ball> onRepeatPass;

    [SerializeField] private GateType gateType;
    [SerializeField] private float value = 1;
   // [SerializeField] private TMP_Text text;

    private HashSet<Ball> _passedBalls = new HashSet<Ball>();

    private void Start()
    {
      //  UpdateGateText();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<Ball>(out var ball)) return;

        if (_passedBalls.Contains(ball))
        {
            onRepeatPass?.Invoke(ball);
            return;
        }

        switch (gateType)
        {
            case GateType.Add:
                ball.AddPrice(Mathf.RoundToInt(value));
                break;
            case GateType.Multiply:
                ball.MultiplyPrice(value);
                break;
            case GateType.Subtract:
                ball.SubtractPrice(Mathf.RoundToInt(value));
                break;
        }

        _passedBalls.Add(ball);
        ball.OnDespawned += HandleBallDespawned; // Subscribe to despawn event
    }

    private void HandleBallDespawned(Ball ball)
    {
        // Remove ball from passed list when it despawns
        if (_passedBalls.Contains(ball))
        {
            _passedBalls.Remove(ball);
            ball.OnDespawned -= HandleBallDespawned; // Unsubscribe
        }
    }

    //private void UpdateGateText()
    //{
    //    if (!text) return;

    //    string sign = gateType switch
    //    {
    //        GateType.Add => "+",
    //        GateType.Subtract => "-",
    //        GateType.Multiply => "x",
    //        _ => ""
    //    };

    //    text.text = $"{sign}{value}";
    //}

    private void OnDestroy()
    {
        // Clean up subscriptions
        foreach (var ball in _passedBalls)
        {
            if (ball != null)
            {
                ball.OnDespawned -= HandleBallDespawned;
            }
        }
    }
}