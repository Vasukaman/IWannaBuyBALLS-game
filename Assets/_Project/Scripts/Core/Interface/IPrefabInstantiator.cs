// Filename: IPrefabInstantiator.cs
using UnityEngine;

namespace Core.Spawning
{
    /// <summary>
    /// Defines a contract for a component that can instantiate prefabs.
    /// This allows pure C# services to create GameObjects without directly
    /// depending on the Unity engine's Instantiate method.
    /// </summary>
    public interface IPrefabInstantiator
    {
        GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Transform parent = null);
    }
}