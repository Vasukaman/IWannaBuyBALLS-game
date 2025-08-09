// Filename: SellZoneProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewSellZoneProfile", menuName = "Profiles/Gadgets/Sell Zone (Master)")]
public class SellZoneProfile : ScriptableObject
{
    [Header("Presenter Logic")]
    [Tooltip("The time in seconds a gadget must be fully in the zone to be sold.")]
    public float TimeToSell = 3.0f;
    [Tooltip("The maximum distance the zone will track a gadget before resetting.")]
    public float MaxTrackingDistance = 5.0f;
    [Tooltip("How much extra space to add around the gadget when capturing it.")]
    public float CapturePadding = 0.5f;

    [Header("Component Profiles")]
    [Tooltip("The profile that defines the visual appearance of the zone.")]
    public SellZoneViewProfile View;
}