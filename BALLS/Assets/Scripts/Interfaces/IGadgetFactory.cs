using UnityEngine;
using Gameplay.Gadgets;
public interface IGadgetFactory
{
    GameObject CreateGadget(GadgetData data, Vector3 position);
}

