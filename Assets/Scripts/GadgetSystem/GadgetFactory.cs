// Filename: GadgetFactory.cs
using UnityEngine;

namespace Gameplay.Gadgets
{

    public class GadgetFactory : MonoBehaviour, IGadgetFactory
    {
        /// <summary>
        /// Creates a new gadget instance in the scene from its ScriptableObject data.
        /// </summary>
        public GameObject CreateGadget(GadgetData data, Vector3 position)
        {
            if (data.Prefab == null)
            {
                Debug.LogError($"Gadget prefab for '{data.DisplayName}' is null!");
                return null;
            }

            GameObject gadgetInstance = Instantiate(data.Prefab, position, Quaternion.identity);

            // TODO: [Robustness] This logic is defensive. A better approach is to ensure the prefab
            // *always* has the Gadget component attached, which simplifies this code and prevents
            // runtime performance hits from AddComponent.
            Gadget gadgetComponent = gadgetInstance.GetComponent<Gadget>();
            if (gadgetComponent == null)
            {
                gadgetComponent = gadgetInstance.AddComponent<Gadget>();
            }

            gadgetComponent.Initialize(data);

            return gadgetInstance;
        }
    }
}