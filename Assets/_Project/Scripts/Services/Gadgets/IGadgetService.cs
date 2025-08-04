// Filename: IGadgetService.cs
using Gameplay.Gadgets;
using UnityEngine;
using Core.Interfaces;
using Core.Data;

namespace Services.Gadgets
{
    public interface IGadgetService
    {
        // The service's job is to create a fully initialized PlaceableView
        IPlaceableView CreateGadget(GadgetData data, Vector3 position, Transform parent = null);
    }
}