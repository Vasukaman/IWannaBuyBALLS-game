// Filename: LiquidGateProfile.cs
using UnityEngine;

// This attribute lets you right-click in the Project window to create new profiles.
[CreateAssetMenu(fileName = "NewLiquidGateProfile", menuName = "VFX/Liquid Gate Profile")]
public class LiquidGateProfile : ScriptableObject
{


    [Header("Bulge")]
    public float MaxBulge = 1f;
    public float MaxBulgePos = 0.8f;
    public float MinBulgePos = 0.8f;

    [Header("Wobble")]
    public float Spring = 90f;
    public float Damper = 10f;
    public float LerpSpeed = 4f;

    [Header("Tilt & Noise")]
    public float VelocitySensitivity = 0.5f;
    public float MaxVelocityBulge = 0.8f;
    public float NoiseAmplitude = 0.05f;
    public float MaxTiltAngle = 35f;
    public float TiltLerpSpeed = 5f;

    [Header("X-Ray Effect")]
    public bool XrayForTopZone = true;
    public Color XrayColorTop = new Color(0, 0.1f, 0.2f, 1);
    public Color HighlightColorTop = new Color(0.5f, 1, 1, 1);
    [Space]
    public bool XrayForBottomZone = true;
    public Color XrayColorBottom = new Color(0.2f, 0, 0.1f, 1);
    public Color HighlightColorBottom = new Color(1, 0.5f, 0.8f, 1);
}