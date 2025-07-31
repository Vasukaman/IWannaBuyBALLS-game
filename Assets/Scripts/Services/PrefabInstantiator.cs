// Filename: PrefabInstantiator.cs
using UnityEngine;

namespace Core.Spawning
{
    /// <summary>
    /// A simple MonoBehaviour that acts as a "worker" for services.
    /// Its sole responsibility is to instantiate prefabs in the Unity scene.
    /// </summary>
    public class PrefabInstantiator : MonoBehaviour, IPrefabInstantiator
    {
        public GameObject InstantiatePrefab(GameObject prefab, Vector3 position, Transform parent = null)
        {
            return Instantiate(prefab, position, Quaternion.identity, parent);
        }
    }
}