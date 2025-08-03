using Gameplay.Gadgets.Effects;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGateProfile", menuName = "Profiles/Gadgets/Gate Profile")]
public class GateProfile : ScriptableObject
{
    [Header("Gate Effect")]
    // We apply both attributes to our interface field.
    [SerializeReference, SelectImplementation(typeof(IGateEffect))]
    public IGateEffect Effect;
}