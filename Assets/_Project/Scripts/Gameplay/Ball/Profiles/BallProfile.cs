// Filename: BallProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBallProfile", menuName = "Profiles/Ball/Master Ball Profile")]
public class BallProfile : ScriptableObject
{
    [Header("Core Attributes")]
    [Tooltip("The initial price of the ball when it is first created.")]
    public int BasePrice = 1;

    [Header("Component Profiles")]
    public BallScalingProfile Scaling;
    public BallMergingProfile Merging;
    public BallAppearanceProfile Appearance;
    public BallProximityProfile Proximity;
}