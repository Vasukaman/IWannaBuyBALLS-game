using Game.Economy;
using Reflex.Attributes;
using UnityEngine;

public class Seller : MonoBehaviour
{
    [Inject] private IMoneyService _moneyService;
    private void OnTriggerEnter2D(Collider2D other)
    {
        
         Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            _moneyService.Add(ball.CurrentPrice);
            ball.Despawn();
        }
    }
}
