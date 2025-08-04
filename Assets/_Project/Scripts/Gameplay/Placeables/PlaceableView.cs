// Filename: PlaceableView.cs
using Gameplay.Interfaces; // For ISellable
using Reflex.Attributes;
using Services.Store;
using UnityEngine;
using Gameplay.Gadgets;
using Core.Data;
using Core.Interfaces;

namespace Gameplay.Placeables
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlaceableView : MonoBehaviour, IGadgetSellable, IPlaceableView
    {
        // --- Injected Dependencies ---
        // The View now gets the service it needs to create its Model.
        [Inject] private IStoreService _storeService;

        // --- Properties ---
        public PlaceableModel Model { get; private set; }
        public GadgetData Data { get; private set; } // It also holds a direct reference to its blueprint

        // --- ISellable Implementation ---
        public GameObject Instance => gameObject;
        public Collider2D ObjectCollider { get; private set; }

        public void Sell() => Model?.Sell();

        // --- Unity Methods ---
        private void Awake()
        {
            ObjectCollider = GetComponent<BoxCollider2D>();
        }

        private void OnDestroy()
        {
            if (Model != null)
            {
                Model.OnDestroyRequested -= HandleDestroyRequested;
            }
        }

        // --- Public API ---

        /// <summary>
        /// Initializes the View with its blueprint data. Called by the GadgetService.
        /// It then creates its own Model.
        /// </summary>
        public void Initialize(GadgetData data)
        {
            Data = data;

            // The View is now responsible for creating its own brain,
            // using its injected dependencies and the data it was given.
            Model = new PlaceableModel(Data, _storeService);
            Model.OnDestroyRequested += HandleDestroyRequested;
            
        }

        private void HandleDestroyRequested()
        {
            Destroy(gameObject);
        }
    }
}