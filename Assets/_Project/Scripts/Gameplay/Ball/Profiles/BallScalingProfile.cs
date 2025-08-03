// Filename: BallScalingProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBallScalingProfile", menuName = "Profiles/Ball/Scaling Profile")]
public class BallScalingProfile : ScriptableObject
{
    [Header("Scaling Logic")]
    public float MinScale = 0.1f;
    public float BaseScale = 0.5f;
    public float ScaleFactor = 0.5f;
    public float MaxScale = 5.0f;

    [Header("Animation")]
    public float ScaleAnimationSpeed = 8f;
}   