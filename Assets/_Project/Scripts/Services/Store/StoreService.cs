// Filename: StoreService.cs
using Services.Money;
using Gameplay.Gadgets;
using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Data;

namespace Services.Store
{
    public class StoreService : IStoreService
    {
        public event Action OnStoreStateChanged;
        public IReadOnlyList<GadgetData> AvailableGadgets => _gadgetLibrary.AllGadgets;

        private readonly IMoneyService _moneyService;
        private readonly GadgetLibrary _gadgetLibrary;
        private readonly Dictionary<GadgetData, int> _purchasedGadgetCounts = new();


        // Dependencies are passed in when the service is created by the DI container.
        public StoreService(IMoneyService moneyService, GadgetLibrary gadgetLibrary)
        {
            _moneyService = moneyService;
            _gadgetLibrary = gadgetLibrary;
        }

        public int GetCurrentPrice(GadgetData data)
        {
            _purchasedGadgetCounts.TryGetValue(data, out int count);
            return data.Price * (int)Mathf.Pow(2, count);
        }

        public bool CanAfford(GadgetData data)
        {
            return _moneyService.CurrentBalance >= GetCurrentPrice(data);
        }

        public bool PurchaseItem(GadgetData data)
        {
            if (!CanAfford(data)) return false;

            int price = GetCurrentPrice(data);
            _moneyService.Spend(price);
            _purchasedGadgetCounts[data] = _purchasedGadgetCounts.GetValueOrDefault(data, 0) + 1;

            // Notify any listeners (like the UI) that something has changed.
            OnStoreStateChanged?.Invoke();
            return true;
        }

        public void RefundPurchase(GadgetData data)
        {
            int currentCount = _purchasedGadgetCounts.GetValueOrDefault(data, 0);
            if (currentCount <= 0) return;

            _purchasedGadgetCounts[data] = currentCount - 1;
            int priceToRefund = GetCurrentPrice(data);
            _moneyService.Add(priceToRefund);

            // Notify any listeners that something has changed.
            OnStoreStateChanged?.Invoke();
        }
    }
}