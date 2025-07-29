using UnityEngine;
public interface IGadgetFactory
{
    GameObject CreateGadget(GadgetData data, Vector3 position);
}

