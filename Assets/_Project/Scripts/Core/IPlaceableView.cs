// Filename: IPlaceableView.cs
// Location: _Project/Scripts/Core/Interfaces/
using Gameplay.Gadgets;
using UnityEngine;
using Core.Data;

namespace Core.Interfaces
{
    /// <summary>
    /// The contract for any MonoBehaviour that represents a placeable gadget.
    /// This allows the GadgetService to create and initialize gadgets without
    /// depending on the concrete implementation in the Gameplay assembly.
    /// </summary>
    public interface IPlaceableView
    {
        // The service needs access to the GameObject and Transform for lifecycle management.
        GameObject gameObject { get; }
        Transform transform { get; }

        // The service needs to be able to initialize the view with its model.
        // We have a problem here: PlaceableModel is in the Gameplay assembly.
        // For now, we will solve this with a temporary object parameter.
        void Initialize(GadgetData data);
    }
}