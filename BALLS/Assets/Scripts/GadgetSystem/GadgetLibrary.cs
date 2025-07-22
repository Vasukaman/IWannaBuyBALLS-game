using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/GadgetLibrary")]
public class GadgetLibrary : ScriptableObject
{
    public List<GadgetData> gadgets;
}
