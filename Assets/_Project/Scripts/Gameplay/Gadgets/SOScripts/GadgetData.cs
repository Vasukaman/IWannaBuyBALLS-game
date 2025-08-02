// Filename: GadgetData.cs
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A ScriptableObject that defines the static properties of a single gadget.
    /// This allows for easy creation and management of different gadget types.
    /// </summary>
    [CreateAssetMenu(fileName = "NewGadget", menuName = "Gameplay/Gadget Data")]
    public class GadgetData : ScriptableObject
    {
        [Tooltip("The name displayed in the UI.")]
        public string DisplayName;

        [Tooltip("The base cost of the gadget in the store.")]
        public int Price;

        [Tooltip("The prefab that will be instantiated for this gadget.")]
        public GameObject Prefab;

        [Tooltip("The icon used in the shop UI.")]
        public Sprite Icon;
    }
}