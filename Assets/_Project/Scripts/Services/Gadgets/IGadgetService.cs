// Filename: IGadgetService.cs
using Gameplay.Gadgets;
using UnityEngine;

namespace Services.Gadgets
{
    public interface IGadgetService
    {
        // The service's job is to create a fully initialized PlaceableView
        PlaceableView CreateGadget(GadgetData data, Vector3 position, Transform parent = null);
    }
}