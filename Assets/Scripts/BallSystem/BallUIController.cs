// Filename: BallUIController.cs
using TMPro;
using UnityEngine;

namespace Gameplay.BallSystem
{
    /// <summary>
    /// Listens to a Ball's OnPriceChanged event and updates a TextMeshPro component.
    /// This keeps UI logic separate from the ball's core data model.
    /// </summary>
    [RequireComponent(typeof(BallView))]
    public class BallUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TMP_Text _priceText;

        private BallView _ball;

        private void Awake()
        {
            _ball = GetComponent<BallView>();

            // Subscribe to the event to receive updates
           
        }

        private void OnEnable()
        {
            if (_ball.Data != null) //TODO:THis can break stuff. but bc we are doing pooling, it doesn't. I should be careful.
            _ball.Data.OnPriceChanged += HandlePriceChanged;

            _ball.OnInitialize += HandleInitialized;
        }

        private void OnDisable()
        {
            _ball.Data.OnPriceChanged -= HandlePriceChanged;
            _ball.OnInitialize -= HandleInitialized;
        }

        private void OnDestroy()
        {
            // Always unsubscribe from events when the object is destroyed to prevent errors
            if (_ball != null)
            {
                _ball.Data.OnPriceChanged -= HandlePriceChanged;
                _ball.OnInitialize -= HandleInitialized;
            }
        }

        /// <summary>
        /// Called when the ball is first initialized to set the starting text.
        /// </summary>
        private void HandleInitialized(BallView ball)
        {
            UpdatePriceText(ball.Data.CurrentPrice);
        }

        /// <summary>
        /// Updates the text component with the new price.
        /// </summary>
        private void HandlePriceChanged(int newPrice)
        {
            UpdatePriceText(newPrice);
        }

        private void UpdatePriceText(int price)
        {
            if (_priceText != null)
            {
                _priceText.text = price.ToString();
            }
        }
    }
}