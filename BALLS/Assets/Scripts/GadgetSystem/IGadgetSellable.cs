using UnityEngine;

 /// <summary>
    /// Defines the contract for any in-game object that can be sold.
    /// It provides a mechanism to handle the selling process and retrieve necessary data.
    /// </summary>
    public interface IGadgetSellable
    {
        /// <summary>
        /// The GameObject representation of the sellable item.
        /// </summary>
        GameObject Instance { get; }

        /// <summary>
        /// The price for which this item will be sold.
        /// </summary>
        int SellPrice { get; }

        /// <summary>
        /// The renderer used to apply visual effects, like the selling shader.
        /// </summary>
        Renderer ObjectRenderer { get; }

        /// <summary>
        /// The collider that defines the physical bounds of the item.
        /// </summary>
        Collider2D ObjectCollider { get; }

        /// <summary>
        /// Executes the logic for selling the item.
        /// This typically involves adding currency and destroying the object.
        /// </summary>
        void Sell();
    }