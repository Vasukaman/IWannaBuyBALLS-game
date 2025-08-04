// Filename: IStoreService.cs
using Gameplay.Gadgets;
using System;
using System.Collections.Generic;
using Core.Data;

namespace Services.Store
{
    public interface IStoreService
    {
        event Action OnStoreStateChanged;
        IReadOnlyList<GadgetData> AvailableGadgets { get; }

        int GetCurrentPrice(GadgetData data);
        bool CanAfford(GadgetData data);
        bool PurchaseItem(GadgetData data);
        void RefundPurchase(GadgetData data);
    }
}