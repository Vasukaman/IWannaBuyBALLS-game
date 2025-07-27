using UnityEngine;

[RequireComponent(typeof(Ball))]
public class BallScaler : MonoBehaviour
{
    [SerializeField] private float minScale = 0.1f; // The absolute minimum scale the ball can have
    [SerializeField] private float baseScale = 0.5f; // Scale at price 1, before considering minScale
    [SerializeField] private float scaleFactor = 0.5f; // How much logarithm contributes to scale
    [SerializeField] private float maxScale = 5.0f;    // Explicit maximum scale to prevent absurdity

    private Ball _ball;

    private void Awake()
    {
        _ball = GetComponent<Ball>();
        _ball.OnPriceChanged += HandlePriceChanged;
        HandlePriceChanged(_ball.CurrentPrice); // Set scale at start
    }

    private void OnDestroy()
    {
        if (_ball != null)
            _ball.OnPriceChanged -= HandlePriceChanged;
    }

    private void HandlePriceChanged(int newPrice)
    {
        // Ensure newPrice is at least 1 to avoid issues with log(0) or log(negative).
        // For newPrice = 1, Mathf.Log(1) is 0, so scale would be baseScale if not clamped.
        // For newPrice > 1, the log value increases.
        float logScaledValue = scaleFactor * Mathf.Log(Mathf.Max(1, newPrice));

        // Add the base scale
        float calculatedScale = baseScale + logScaledValue;

        // Clamp the final scale between minScale and maxScale
        float finalScale = Mathf.Clamp(calculatedScale, minScale, maxScale);

        transform.localScale = Vector3.one * finalScale;
    }
}