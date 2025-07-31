// Filename: GadgetView.cs (replaces Gadget.cs)
using Reflex.Attributes;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// The MonoBehaviour component for a Gadget. It acts as the "bridge" between the
    /// pure C# GadgetModel and the Unity engine (physics, rendering, lifecycle).
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class GadgetView : MonoBehaviour, IGadgetSellable
    {
        // --- Properties ---
        public GadgetModel Model { get; private set; }

        // --- IGadgetSellable Implementation ---
        public GameObject Instance => gameObject;
        public Collider2D ObjectCollider { get; private set; }

        // --- Unity Methods ---
        private void Awake()
        {
            ObjectCollider = GetComponent<Collider2D>();
        }

        private void OnDestroy()
        {
            // Clean up event subscriptions when the object is destroyed.
            if (Model != null)
            {
                Model.OnDestroyRequested -= HandleDestroyRequested;
            }
        }

        // --- Public API ---

        /// <summary>
        /// Initializes the View by connecting it to its Model.
        /// Called by the GadgetService upon creation.
        /// </summary>
        public void Initialize(GadgetModel model)
        {
            Model = model;
            Model.OnDestroyRequested += HandleDestroyRequested;
        }

        /// <summary>
        /// Public method for other systems to call. It delegates the logic to the Model.
        /// </summary>
        public void Sell()
        {
            Model?.Sell();
        }

        // --- Event Handlers ---

        /// <summary>
        /// Listens for the event from the Model and performs the Unity-specific action.
        /// </summary>
        private void HandleDestroyRequested()
        {
            // This is the only place that calls Destroy(). The logic is now separate.
            Destroy(gameObject);
        }
    }
}