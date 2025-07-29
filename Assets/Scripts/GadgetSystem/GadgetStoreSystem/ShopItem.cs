// Filename: StoreItemView.cs
using Gameplay.Gadgets;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Store
{
    /// <summary>
    /// Manages the visual representation of a single item in the store UI.
    /// It displays the gadget's data and updates its appearance based on affordability.
    /// </summary>
    [RequireComponent(typeof(Button), typeof(CanvasGroup))]
    public class StoreItemView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Color _canAffordColor = Color.green;
        [SerializeField] private Color _cannotAffordColor = Color.red;

        // --- State & Cache ---
        private GadgetData _gadgetData;
  
        private CanvasGroup _canvasGroup;

        // --- Unity Methods ---

        private void Awake()
        {
    
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        // --- Public API ---

        /// <summary>
        /// Sets up the initial state of the store item. Called by the StoreManager.
        /// </summary>
        /// <param name="data">The ScriptableObject data for the gadget to display.</param>
        /// <param name="onPurchaseAttempt">The action to call when this item is clicked.</param>
        public void Initialize(GadgetData data, Action<GadgetData> onPurchaseAttempt)
        {
            _gadgetData = data;

            // Populate the UI elements
            _iconImage.sprite = _gadgetData.Icon;
            _nameText.text = _gadgetData.DisplayName;


        }

        /// <summary>
        /// Updates the item's display with the current price and affordability.
        /// </summary>
        /// <param name="price">The current calculated price of the gadget.</param>
        /// <param name="canAfford">Whether the player can afford to purchase it.</param>
        public void UpdateDisplay(int price, bool canAfford)
        {
            _priceText.text = price.ToString();
            _priceText.color = canAfford ? _canAffordColor : _cannotAffordColor;


            _canvasGroup.alpha = canAfford ? 1f : 0.6f;
        }
    }
}