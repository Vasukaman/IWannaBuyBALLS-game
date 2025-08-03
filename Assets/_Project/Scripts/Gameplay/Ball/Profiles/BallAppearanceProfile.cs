// Filename: BallAppearanceProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBallAppearanceProfile", menuName = "Profiles/Ball/Appearance Profile")]
public class BallAppearanceProfile : ScriptableObject
{
    [Header("Color")]
    public Color BaseColor = Color.white;

    [Header("Tier 1 Orb Settings")]
    public float OrbSpeed = 2.0f;
    public float BaseOrbRadius = 0.04f;

    [Header("Tier 2 Orb Settings")]
    public int Tier2Threshold = 50;
    public float Tier2OrbSpeed = -1.0f;
    public float Tier2BaseOrbRadius = 0.08f;

    [Header("General Appearance")]
    public float BaseOutlineThickness = 0.05f;
    public bool ShowPathLine = false;
    public float BasePathLineThickness = 0.005f;
}