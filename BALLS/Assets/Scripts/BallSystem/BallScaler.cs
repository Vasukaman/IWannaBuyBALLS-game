using UnityEngine;

[RequireComponent(typeof(Ball))]
public class BallScaler : MonoBehaviour
{
    [Header("Scaling Logic")]
    [SerializeField] private float minScale = 0.1f;      // The absolute minimum scale the ball can have
    [SerializeField] private float baseScale = 0.5f;     // Scale at price 1, before considering minScale
    [SerializeField] private float scaleFactor = 0.5f;   // How much logarithm contributes to scale
    [SerializeField] private float maxScale = 5.0f;      // Explicit maximum scale to prevent absurdity

    [Header("Animation")]
    [Tooltip("How quickly the ball animates to its new size.")]
    [SerializeField] private float scaleSpeed = 8f;

    private Ball _ball;
    private Vector3 _targetScale; // The scale we are animating towards

    private void Awake()
    {
        _ball = GetComponent<Ball>();
        _ball.OnPriceChanged += HandlePriceChanged;

        // Calculate and set the initial scale immediately without animation
        float initialScaleValue = CalculateScaleForPrice(_ball.CurrentPrice);
        transform.localScale = Vector3.one * initialScaleValue;
        _targetScale = transform.localScale; // Sync target scale to prevent animation on start
    }

    private void OnDestroy()
    {
        if (_ball != null)
            _ball.OnPriceChanged -= HandlePriceChanged;
    }

    private void Update()
    {
        // In each frame, smoothly move the current scale towards the target scale.
        // This creates the smooth scaling animation.
        transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * scaleSpeed);
    }

    /// <summary>
    /// This is called when the ball's price changes. It sets the new target scale.
    /// </summary>
    private void HandlePriceChanged(int newPrice)
    {
        float finalScaleValue = CalculateScaleForPrice(newPrice);
        _targetScale = Vector3.one * finalScaleValue;
    }

    /// <summary>
    /// Calculates the desired scale based on a given price.
    /// </summary>
    private float CalculateScaleForPrice(int price)
    {
        // Ensure newPrice is at least 1 to avoid issues with log(0) or log(negative).
        float logScaledValue = scaleFactor * Mathf.Log(Mathf.Max(1, price));

        // Add the base scale
        float calculatedScale = baseScale + logScaledValue;

        // Clamp the final scale between minScale and maxScale
        return Mathf.Clamp(calculatedScale, minScale, maxScale);
    }
}