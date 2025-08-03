// Filename: BallScaler.cs
using Gameplay.BallSystem;
using UnityEngine;

namespace Gameplay.BallSystem
{
    /// <summary>
    /// Manages the visual scale of the ball based on its price.
    /// It listens for price changes and smoothly animates the ball to its new size.
    /// </summary>
    [RequireComponent(typeof(BallView))]
    public class BallScaler : MonoBehaviour
    {

        private BallView _ballView;
        private BallScalingProfile _scalingProfile;

        private Vector3 _targetScale;

        // --- Unity Methods ---

        private void Awake()
        {
            // 1. Get the root component on this GameObject.
            _ballView = GetComponent<BallView>();

            // 2. Pull the specific profile it needs from the master profile.
            if (_ballView.Profile != null)
            {
                _scalingProfile = _ballView.Profile.Scaling;
            }

            // 3. Validate that the required data exists.
            if (_scalingProfile == null)
            {
                Debug.LogError("BallScalingProfile is not assigned in the master BallProfile! Disabling BallScaler.", this);
                enabled = false; // Disable this component to prevent errors.
            }
        }

        private void OnEnable()
        {
            // Subscribing in OnEnable is the most robust pattern for pooled objects.
            // It ensures we are listening whenever the object is active.
            if (_ballView != null)
            {
                _ballView.OnInitialize += HandleInitialized;

                // We must check if the Data model exists before subscribing.
                if (_ballView.Data != null)
                {
                    _ballView.Data.OnPriceChanged += HandlePriceChanged;
                }
            }
        }

        private void OnDisable()
        {
            // Always unsubscribe in OnDisable to match OnEnable subscriptions.
            if (_ballView != null)
            {
                _ballView.OnInitialize -= HandleInitialized;
                if (_ballView.Data != null)
                {
                    _ballView.Data.OnPriceChanged -= HandlePriceChanged;
                }
            }
        }

        private void Update()
        {
            // Avoids running the Lerp if the scale is already at its target.
            if (transform.localScale == _targetScale) return;

            // TODO: This Lerp is frame-rate dependent. For more precise animation, consider
            // using Vector3.MoveTowards or a dedicated tweening library (like DOTween/LeanTween).
            // For this project's style, this approach is simple and visually effective.
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.deltaTime * _scalingProfile.ScaleAnimationSpeed);
        }

        // --- Event Handlers ---

        /// <summary>
        /// Responds to the ball being initialized, setting its scale immediately without animation.
        /// This is crucial for when the ball is recycled from an object pool.
        /// </summary>
        private void HandleInitialized(BallView initializedBall)
        {
            // It's possible the Data object was just created, so we subscribe here as well
            // to be absolutely safe. We remove first to prevent double-subscription.
            if (initializedBall.Data != null)
            {
                initializedBall.Data.OnPriceChanged -= HandlePriceChanged;
                initializedBall.Data.OnPriceChanged += HandlePriceChanged;
            }

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
            if (_ballView.Data == null) return;

            float initialScaleValue = CalculateScaleForPrice(_ballView.Data.CurrentPrice);
            _targetScale = Vector3.one * initialScaleValue;
            transform.localScale = _targetScale; // Set scale directly, no animation.
        }

        /// <summary>
        /// The core logic for determining the scale from a given price value.
        /// </summary>
        private float CalculateScaleForPrice(int price)
        {
            // Using Log provides a nice curve where scale increases are larger at lower prices
            // and smaller at higher prices, preventing runaway sizes.
            float logScaledValue = _scalingProfile.ScaleFactor * Mathf.Log(Mathf.Max(1, price));
            float calculatedScale = _scalingProfile.BaseScale + logScaledValue;

            return Mathf.Clamp(calculatedScale, _scalingProfile.MinScale, _scalingProfile.MaxScale);
        }
    }
}