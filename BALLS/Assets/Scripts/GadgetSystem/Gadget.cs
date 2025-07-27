using Game.Economy;
using Reflex.Attributes;
using UnityEngine;


/// <summary>
/// Represents a gadget that can be purchased and sold.
/// Implements IGadgetSellable to interact with selling zones.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Gadget : MonoBehaviour, IGadgetSellable
{
    [Tooltip("Scriptable Object containing the base data for this gadget.")]
    public GadgetData Data;

    // --- Injected Dependencies ---
    [Inject] private IMoneyService _moneyService;

    // --- IGadgetSellable Implementation ---
    public GameObject Instance => gameObject;
    public int SellPrice => PurchasePrice; // For now, sell price is the same as purchase price.
    public Renderer ObjectRenderer { get; private set; }
    public Collider2D ObjectCollider { get; private set; }

    /// <summary>
    /// The price at which this gadget was originally purchased.
    /// </summary>
    public int PurchasePrice { get; private set; }


    private void Awake()
    {
        // Cache the components on Awake for performance.
        ObjectRenderer = GetComponent<Renderer>();
        ObjectCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Initializes the gadget with its data and purchase price.
    /// </summary>
    /// <param name="data">The gadget's Scriptable Object data.</param>
    /// <param name="purchasePrice">The price paid by the player.</param>
    public void Initialize(GadgetData data, int purchasePrice)
    {
        Data = data;
        PurchasePrice = purchasePrice;
        // You would also set the sprite from the GadgetData here.
        // GetComponent<SpriteRenderer>().sprite = Data.GadgetSprite;
    }

    /// <summary>
    /// Sells the gadget, adding its value to the player's balance
    /// and removing it from the game.
    /// </summary>
    public void Sell()
    {
        // Use the injected money service to add the funds.
   //     _moneyService.Add(SellPrice);

        // Here you might play a particle effect or a sound before destroying.
   //     Debug.Log($"Sold {Data.Name} for {SellPrice}!");

        // Remove the gadget from the game world.
        Destroy(gameObject);
    }
}


