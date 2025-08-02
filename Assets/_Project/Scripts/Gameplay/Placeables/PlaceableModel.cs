// Filename: PlaceableModel.cs
using Gameplay.Gadgets;
using Services.Store;
using System;
using UnityEngine.Playables;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// The pure data model for a Gadget. Contains all state and logic
    /// that is independent of the Unity engine.
    /// </summary>
    public class PlaceableModel
    {
        public event Action OnDestroyRequested;

        public GadgetData Data { get; }

        private readonly IStoreService _storeService;


        // The service dependency is passed in when the model is created.
        public PlaceableModel(GadgetData data, IStoreService storeService)
        {
            Data = data;
            _storeService = storeService;
        }

        /// <summary>
        /// Executes the logic for selling the gadget.
        /// </summary>
        public void Sell()
        {
            if (Data != null)
            {
                _storeService.RefundPurchase(Data);
            }

            // Instead of destroying, it fires an event to notify the View.
            OnDestroyRequested?.Invoke();
        }
    }
}