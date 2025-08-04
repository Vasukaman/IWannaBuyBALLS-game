// Filename: StoreView.cs (replaces StoreManager.cs)
using Gameplay.Gadgets;
using Reflex.Attributes;
using Reflex.Core;
using Reflex.Injectors;
using Services.Store; // Use the new service namespace
using System.Collections.Generic;
using UI.Shop;
using UI.Store;
using UnityEngine;
using Core.Data;


namespace UI.Shop
{
    /// <summary>
    /// Manages the UI presentation of the store. It creates the visual slots for each item
    /// and updates them when the underlying store data changes.
    /// </summary>
    public class StoreView : MonoBehaviour
    {
        [Header("UI Prefabs & Containers")]
        [SerializeField] private GameObject _shopItemPrefab;
        [SerializeField] private Transform _shopItemsContainer;

        // --- Injected Dependencies ---
        [Inject] private IStoreService _storeService;
        [Inject] private Container _container; // For injecting into runtime objects

        // --- State ---
        private readonly List<StoreItemPresenter> _shopItemControllers = new();

        // --- Unity Methods ---

        private void Start()
        {
            InitializeShopUI();
            // Subscribe to the service's event to know when to refresh.
            _storeService.OnStoreStateChanged += RefreshAllShopItems;
        }

        private void OnDestroy()
        {
            // Always unsubscribe from events!
            if (_storeService != null)
            {
                _storeService.OnStoreStateChanged -= RefreshAllShopItems;
            }
        }

        // --- Private UI Management ---

        private void InitializeShopUI()
        {
            // Clear any existing items from the editor.
            foreach (Transform child in _shopItemsContainer)
            {
                Destroy(child.gameObject);
            }
            _shopItemControllers.Clear();

            // Ask the service for the list of available gadgets.
            foreach (GadgetData gadgetData in _storeService.AvailableGadgets)
            {
                // Create the UI prefab.
                GameObject itemGO = Instantiate(_shopItemPrefab, _shopItemsContainer);

                // Manually inject dependencies into the new UI object.
     

                GameObjectInjector.InjectRecursive(itemGO, _container);

                StoreItemPresenter controller = itemGO.GetComponentInChildren<StoreItemPresenter>();

                if (controller != null)
                {
                    controller.Initialize(gadgetData);
                    _shopItemControllers.Add(controller);
                }
                else
                {
                    Debug.LogError($"ShopItemPrefab is missing the ShopItemController script!", itemGO);
                }

                //if (itemGO.TryGetComponent<StoreSlotController>(out var controller))
                //{
                //    // The controller no longer needs the StoreManager/View.
                //    // It will get the IStoreService injected directly by the line above.
                //   //controller.Initialize(gadgetData);
                //    _shopItemControllers.Add(controller);
                //}
            }
        }

        private void RefreshAllShopItems()
        {
            foreach (var itemController in _shopItemControllers)
            {
                itemController.UpdateDisplay();
            }
        }
    }
}