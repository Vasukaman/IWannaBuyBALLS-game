// Filename: BallScaler.cs
using Gameplay.BallSystem;
using UnityEngine;

namespace Gameplay.BallSystem
{
    /// <summary>
    /// Manages the visual scale of the ball based on its price.
    /// It listens for price changes and smoothly animates the ball to its new size.
    /// </summary>
    [RequireComponent(typeof(Ball))]
    public class BallScaler : MonoBehaviour
    {
        [Header("Scaling Logic")]
        [SerializeField] private float _minScale = 0.1f;
        [SerializeField] private float _baseScale = 0.5f;
        [SerializeField] private float _scaleFactor = 0.5f;
        [SerializeField] private float _maxScale = 5.0f;

        [Header("Animation")]
        [Tooltip("How quickly the ball animates to its new size.")]
        [SerializeField] private float _scaleAnimationSpeed = 8f;

        private Ball _ball;
        private Vector3 _targetScale;

        // --- Unity Methods ---

        private void Awake()
        {
            _ball = GetComponent<Ball>();
            
            // Subscribe to events
            _ball.OnPriceChanged += HandlePriceChanged;
            _ball.OnInitialize += HandleInitialized;
        }

        private void OnDestroy()
        {
            // Always unsubscribe from events to prevent memory leaks
            if (_ball != null)
            {
                _ball.OnPriceChanged -= HandlePriceChanged;
                _ball.OnInitialize -= HandleInitialized;
            }
        }

        private void Update()
        {
            // TODO: This Lerp is frame-rate dependent. For more precise animation, consider
            // using Vector3.MoveTowards or a dedicated tweening library (like DOTween/LeanTween).
            // For this project's style, this approach is simple and visually effective.
            if (transform.localScale != _targetScale)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * _scaleAnimationSpeed);
            }
        }
        
        // --- Event Handlers ---

        /// <summary>
        /// Responds to the ball being initialized, setting its scale immediately without animation.
        /// This is crucial for when the ball is recycled from an object pool.
        /// </summary>
        private void HandleInitialized(Ball initializedBall)
        {
            SetInitialScale();
        }

        /// <summary>
        /// Responds to the ball's price changing by calculating and setting a new target scale.
        /// </summary>
        private void HandlePriceChanged(int newPrice)
        {
            float finalScaleValue = CalculateScaleForPrice(newPrice);
            _targetScale = Vector3.one * finalScaleValue;
        }
        
        // --- Private Methods ---

        /// <summary>
        /// Calculates and immediately sets the ball's scale based on its current price.
        /// </summary>
        private void SetInitialScale()
        {
            float initialScaleValue = CalculateScaleForPrice(_ball.CurrentPrice);
            _targetScale = Vector3.one * initialScaleValue;
            transform.localScale = _targetScale; // Set scale directly, no animation
        }

        /// <summary>
        /// The core logic for determining the scale from a given price value.
        /// </summary>
        private float CalculateScaleForPrice(int price)
        {
            // Using Log provides a nice curve where scale increases are larger at lower prices
            // and smaller at higher prices, preventing runaway sizes.
            float logScaledValue = _scaleFactor * Mathf.Log(Mathf.Max(1, price));
            float calculatedScale = _baseScale + logScaledValue;

            return Mathf.Clamp(calculatedScale, _minScale, _maxScale);
        }
    }
}