using Game.Economy;
using Reflex.Attributes;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GadgetLibrary _gadgetLibrary;
    [SerializeField] private GameObject _shopItemPrefab;
    [SerializeField] private Transform _shopItemsContainer;

    // This service is injected to handle player currency
    [Inject] private IMoneyService _moneyService;
    // This factory can be used by gadgets once they are placed, if needed
    [Inject] private IGadgetFactory _factory;

    private readonly Dictionary<GadgetData, int> _gadgetCounts = new();
    private readonly List<ShopItemController> _shopItems = new();

    private void Start()
    {
        InitializeShop();
    }

    /// <summary>
    /// Populates the shop with items from the GadgetLibrary.
    /// </summary>
    private void InitializeShop()
    {
        foreach (Transform child in _shopItemsContainer)
        {
            Destroy(child.gameObject);
        }
        _shopItems.Clear();

        // Create a shop item for each gadget in the library
        foreach (GadgetData gadget in _gadgetLibrary.gadgets)
        {
            GameObject itemGO = Instantiate(_shopItemPrefab, _shopItemsContainer);

            ShopItemController controller = itemGO.GetComponentInChildren<ShopItemController>();
            if (controller != null)
            {
                controller.Initialize(gadget, this);
                _shopItems.Add(controller);
            }
            else
            {
                Debug.LogError($"ShopItemPrefab is missing the ShopItemController script!", itemGO);
            }
        }
    }

    /// <summary>
    /// Calculates the price of the next purchase for a given gadget.
    /// The price increases exponentially with each purchase.
    /// </summary>
    public int GetCurrentPrice(GadgetData data)
    {
        _gadgetCounts.TryGetValue(data, out int count);
        // Price = BasePrice * 2^(number already owned)
        return data.price * (int)Mathf.Pow(2, count);
    }

    /// <summary>
    /// Checks if the player has enough money to buy the next version of a gadget.
    /// </summary>
    public bool CanAfford(GadgetData data) =>
        _moneyService.CurrentBalance >= GetCurrentPrice(data);

    /// <summary>
    /// Processes the economic transaction for buying an item.
    /// Called when an item is dragged into the gameplay area.
    /// </summary>
    /// <returns>True if the purchase was successful, otherwise false.</returns>
    public bool PurchaseItem(GadgetData data)
    {
        if (!CanAfford(data)) return false;

        int price = GetCurrentPrice(data);
        _moneyService.Spend(price);

        // Increment the count for this gadget type *after* the purchase
        _gadgetCounts[data] = _gadgetCounts.GetValueOrDefault(data, 0) + 1;

        RefreshShop();
        return true;
    }

    /// <summary>
    /// Reverses a purchase transaction, refunding the player.
    /// Called when an item is dragged from the gameplay area back to the shop UI.
    /// </summary>
    public void RefundPurchase(GadgetData data)
    {
        int currentCount = _gadgetCounts.GetValueOrDefault(data, 0);
        if (currentCount <= 0) return; // Cannot refund an item that hasn't been bought

        // Decrement the count *first* to find the price of the item we are returning
        _gadgetCounts[data] = currentCount - 1;

        // The price to refund is the price calculated with the new, lower count
        int priceToRefund = GetCurrentPrice(data);

        // Add the money back. Assumes your IMoneyService has an 'Add' method.
        // You might need to change this to 'Earn', 'Credit', etc.
        _moneyService.Add(priceToRefund);

        RefreshShop();
    }

    /// <summary>
    /// NOTE: This function is likely obsolete with the new ShopItemController logic.
    /// The 'PurchaseItem' and 'RefundPurchase' methods now handle the transactions,
    /// and the ShopItemController handles the gadget's placement.
    /// </summary>
    public void PlaceGadget(GadgetData data, Vector3 position)
    {
        if (!CanAfford(data)) return;

        int price = GetCurrentPrice(data);
        _moneyService.Spend(price);
        _gadgetCounts[data] = _gadgetCounts.GetValueOrDefault(data, 0) + 1;
        _factory.CreateGadget(data, position);
        RefreshShop();
    }

    /// <summary>
    /// Updates the display of all items in the shop.
    /// </summary>
    public void RefreshShop()
    {
        foreach (var item in _shopItems)
        {
            item.UpdateDisplay();
        }
    }
}
