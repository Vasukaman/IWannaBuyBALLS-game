// Filename: IBallView.cs
// Location: _Project/Scripts/Core/Interfaces/
using System;
using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// The contract for any MonoBehaviour that represents a Ball in the game world.
    /// This allows services to interact with a Ball's core lifecycle without
    /// needing to know about its specific implementation in the Gameplay assembly.
    /// </summary>
    public interface IBallView
    {
        // --- Lifecycle Events ---
        event Action<IBallView> OnRequestDespawn;

        // --- Core Properties ---
        GameObject gameObject { get; }
        Transform transform { get; }

        // --- Initialization Methods ---
        void SetBasePrice(int price);
        void Initialize();
    }
}