using UnityEngine;

public class GadgetFactory : MonoBehaviour, IGadgetFactory
{
    public GameObject CreateGadget(GadgetData data, Vector3 position)
    {
        GameObject gadget = Instantiate(data.prefab, position, Quaternion.identity);

        // Initialize gadget component
        Gadget gadgetComponent = gadget.GetComponent<Gadget>();
        if (gadgetComponent == null) gadgetComponent = gadget.AddComponent<Gadget>();
        gadgetComponent.Initialize(data, 0);

        return gadget;
    }
}