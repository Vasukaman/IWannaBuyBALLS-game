using UnityEngine;

public class Gadget : MonoBehaviour
{
    public GadgetData Data { get; private set; }
    public int PurchasePrice { get; private set; }

    public void Initialize(GadgetData data, int purchasePrice)
    {
        Data = data;
        PurchasePrice = purchasePrice;
    }

    private void OnMouseDown()
    {
        // Will implement selling later
    }
}