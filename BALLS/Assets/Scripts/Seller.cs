using Game.Economy;
using Reflex.Attributes;
using System.Collections;
using UnityEngine;

public class Seller : MonoBehaviour
{
    [Inject] private IMoneyService _moneyService;

    [Tooltip("How long the sucking animation takes in seconds.")]
    [SerializeField] private float animationDuration = 0.3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            StartCoroutine(AnimateAndSellBall(ball));
        }
    }

    private IEnumerator AnimateAndSellBall(Ball ball)
    {
        // Disable the ball's physics so we can control it
        if (ball.TryGetComponent<Collider2D>(out var ballCollider))
        {
           // ballCollider.enabled = false;
        }
        if (ball.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
        }

        // Store initial values
        Vector3 startPosition = ball.transform.position;
        Vector3 startScale = ball.transform.localScale;
        

        float elapsedTime = 0f;

        // Loop over the duration of the animation
        while (elapsedTime < animationDuration)
        {

            Vector3 endPosition = transform.position; // The seller's center
            // Calculate the progress of the animation (from 0 to 1)
            float t = elapsedTime / animationDuration;

            // Smoothly interpolate position and scale
            ball.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            ball.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // Wait for the next frame
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Animation is complete. Give money and despawn the ball.
        _moneyService.Add(ball.CurrentPrice);
        ball.Despawn();
    }
}