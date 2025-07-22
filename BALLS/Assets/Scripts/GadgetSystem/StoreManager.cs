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
    [SerializeField] private float _itemSpacing = 1.5f;

    [Header("Economy")]
    [SerializeField] private int _startingCurrency = 100;


    private Dictionary<GadgetData, int> _gadgetCounts = new();
    [Inject] IMoneyService _moneyService;
    private List<ShopItemController> _shopItems = new();

    [Inject] IGadgetFactory _factory;


    private void Start()
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        // Clear existing items
        foreach (var item in _shopItems)
        {
            Destroy(item.gameObject);
        }
        _shopItems.Clear();

        // Create new shop items
        float xPos = 0;
        foreach (GadgetData gadget in _gadgetLibrary.gadgets)
        {
            GameObject itemGO = Instantiate(_shopItemPrefab, _shopItemsContainer);
            itemGO.transform.localPosition = new Vector3(xPos, 0, 0);

            ShopItemController controller = itemGO.GetComponentInChildren<ShopItemController>();
            controller.Initialize(gadget, this);

            _shopItems.Add(controller);
        //    xPos += _itemSpacing;
        }
    }

    public int GetCurrentPrice(GadgetData data)
    {
        _gadgetCounts.TryGetValue(data, out int count);
        return data.price * (int)Mathf.Pow(2, count);
    }

    public bool CanAfford(GadgetData data) =>
        _moneyService.CurrentBalance >= GetCurrentPrice(data);

    public void PlaceGadget(GadgetData data, Vector3 position)
    {
        if (!CanAfford(data)) return;

        int price = GetCurrentPrice(data);
        _moneyService.Spend(price);
        _gadgetCounts[data] = _gadgetCounts.GetValueOrDefault(data, 0) + 1;

        _factory.CreateGadget(data, position);
        RefreshShop();
    }

    public void RefreshShop()
    {
        foreach (var item in _shopItems)
        {
            item.UpdateDisplay();
        }
    }
}