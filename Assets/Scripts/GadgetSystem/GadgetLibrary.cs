// Filename: GadgetLibrary.cs
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A ScriptableObject that holds a list of all available GadgetData assets in the game.
    /// This acts as a central registry for systems like the shop.
    /// </summary>
    [CreateAssetMenu(fileName = "GadgetLibrary", menuName = "Gameplay/Gadget Library")]
    public class GadgetLibrary : ScriptableObject
    {
        public List<GadgetData> AllGadgets;
    }
}