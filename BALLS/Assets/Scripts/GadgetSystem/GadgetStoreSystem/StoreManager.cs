// Filename: StoreManager.cs
using Game.Economy;
using Gameplay.Gadgets;
using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;


namespace UI.Store
{
    // TODO: [SRP Violation] This manager handles multiple responsibilities: UI generation (InitializeShop),
    // economic calculations (GetCurrentPrice), and state management (_gadgetCounts). In a larger project,
    // the pricing strategy could be extracted into a separate `IPricingService` class to better
    // separate the UI management from the core economic logic.
    public class StoreManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GadgetLibrary _gadgetLibrary;
        [SerializeField] private GameObject _shopItemPrefab;
        [SerializeField] private Transform _shopItemsContainer;

        // --- Injected Dependencies ---
        [Inject] private IMoneyService _moneyService;

        // --- State ---
        private readonly Dictionary<GadgetData, int> _purchasedGadgetCounts = new();
        private readonly List<StoreSlotController> _shopItemControllers = new();

        // --- Unity Methods ---

        private void Start()
        {
            InitializeShop();
        }

        // --- Public API ---

        /// <summary>
        /// Calculates the current purchase price for a given gadget, which increases with each one owned.
        /// </summary>
        public int GetCurrentPrice(GadgetData data)
        {
            _purchasedGadgetCounts.TryGetValue(data, out int count);
            // Price increases exponentially: BasePrice * 2^(number already owned)
            return data.Price * (int)Mathf.Pow(2, count);
        }

        /// <summary>
        /// Checks if the player has enough currency to purchase the next gadget of a given type.
        /// </summary>
        public bool CanAfford(GadgetData data)
        {
            return _moneyService.CurrentBalance >= GetCurrentPrice(data);
        }

        /// <summary>
        /// Processes the transaction for buying an item. Called by the ShopItemController.
        /// </summary>
        /// <returns>True if the purchase was successful, otherwise false.</returns>
        public bool PurchaseItem(GadgetData data)
        {
            if (!CanAfford(data))
            {
                return false;
            }

            int price = GetCurrentPrice(data);
            _moneyService.Spend(price);

            // Increment the count for this gadget type *after* getting the price.
            _purchasedGadgetCounts[data] = _purchasedGadgetCounts.GetValueOrDefault(data, 0) + 1;

            RefreshAllShopItems();
            return true;
        }

        /// <summary>
        /// Processes a refund for a sold gadget. Called by the Gadget component.
        /// </summary>
        public void RefundPurchase(GadgetData data)
        {
            int currentCount = _purchasedGadgetCounts.GetValueOrDefault(data, 0);
            if (currentCount <= 0)
            {
                Debug.LogWarning($"Attempted to refund {data.DisplayName}, but none have been purchased.");
                return;
            }

            // TODO: [Fragile Logic] This refund logic is tightly coupled to the exponential price formula.
            // If the pricing model changes, this refund calculation must also be updated carefully.

            // Decrement the count *first* to calculate the price of the *previous* item.
            _purchasedGadgetCounts[data] = currentCount - 1;
            int priceToRefund = GetCurrentPrice(data);

            _moneyService.Add(priceToRefund);

            RefreshAllShopItems();
        }

        // --- Private Methods ---

        /// <summary>
        /// Clears and repopulates the shop UI based on the GadgetLibrary.
        /// </summary>
        private void InitializeShop()
        {
            ClearShopUI();
            PopulateShopUI();
        }

        /// <summary>
        /// Destroys all existing shop item GameObjects.
        /// </summary>
        private void ClearShopUI()
        {
            foreach (Transform child in _shopItemsContainer)
            {
                Destroy(child.gameObject);
            }
            _shopItemControllers.Clear();
        }

        /// <summary>
        /// Creates and initializes a UI controller for each gadget in the library.
        /// </summary>
        private void PopulateShopUI()
        {
            // TODO: [Performance] This instantiates new UI objects every time. For shops that are
            // frequently opened/closed, using an object pool for the UI elements would be more performant.
            foreach (GadgetData gadgetData in _gadgetLibrary.AllGadgets)
            {
                GameObject itemGO = Instantiate(_shopItemPrefab, _shopItemsContainer);
                StoreSlotController controller = itemGO.GetComponentInChildren<StoreSlotController>();

                if (controller != null)
                {
                    controller.Initialize(gadgetData, this);
                    _shopItemControllers.Add(controller);
                }
                else
                {
                    Debug.LogError($"ShopItemPrefab is missing the ShopItemController script!", itemGO);
                }
            }
        }

        /// <summary>
        /// Tells all active shop items to update their displays (e.g., price text).
        /// </summary>
        private void RefreshAllShopItems()
        {
            foreach (var itemController in _shopItemControllers)
            {
                itemController.UpdateDisplay();
            }
        }
    }
}