// Filename: SellZoneViewProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewSellZoneViewProfile", menuName = "Profiles/VFX/Sell Zone View Profile")]
public class SellZoneViewProfile : ScriptableObject
{
    [Header("Default State Visuals")]
    public Color DefaultDashColor = Color.white;
    public Color DefaultLineColor = new Color(1, 1, 1, 0.1f);
    public Vector2 DefaultLineSpeed = new Vector2(0.1f, 0.1f);
    public float DefaultLineThickness = 0.02f;
    public float DefaultLineSpacing = 0.2f;

    [Header("Selling State Visuals")]
    public Color SellingDashColor = Color.green;
    public Color SellingLineColor = new Color(0, 1, 0, 0.25f);
    public float SellingLineThickness = 3.0f;
    public float SellingLineSpacing = 0.05f;
    public float SpeedAcceleration = 3.0f;
}