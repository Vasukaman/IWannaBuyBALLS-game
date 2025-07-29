// Filename: Gadget.cs
using Reflex.Attributes;
using UnityEngine;
using UI.Store;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// The base component for any interactive object ("gadget") in the game world.
    /// It holds the gadget's data and state, and handles interactions like being sold.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class Gadget : MonoBehaviour, IGadgetSellable
    {
        // --- Properties ---
        public GadgetData Data { get; private set; }


        // --- IGadgetSellable Implementation ---
        public GameObject Instance => gameObject;
   
   
        public Collider2D ObjectCollider { get; private set; }

        // --- Dependencies ---
        // TODO: THIS IS VERY BAD! VERY VERY BAD! I should make a proper runtime injector. 
         public StoreManager _storeManager; 

        // --- Unity Methods ---
        private void Awake()
        {
        
            ObjectCollider = GetComponent<Collider2D>();
        }

        // --- Public API ---

        /// <summary>
        /// Initializes the gadget with its data and the price it was purchased for.
        /// Called by the GadgetFactory upon creation.
        /// </summary>
        public void Initialize(GadgetData data)
        {
            Data = data;
            //PurchasePrice = purchasePrice;


       
        }

        /// <summary>
        /// Sells the gadget and removes it from the game.
        /// </summary>
        public void Sell()
        {
            if (Data != null)
            {
                _storeManager.RefundPurchase(Data);
            }

            // TODO: [Architecture] The gadget is responsible for destroying itself. A more robust
            // pattern might be to fire an OnSellRequested event, allowing a central manager to handle
            // the destruction, pooling, and associated effects (VFX, SFX).
            Destroy(gameObject);
        }
    }
}