// Filename: BallMergingProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBallMergingProfile", menuName = "Profiles/Ball/Merging Profile")]
public class BallMergingProfile : ScriptableObject
{
    [Header("Merge Settings")]
    public float MergeDuration = 0.35f;
    public float MergeCooldownAfterSpawn = 0.5f;
    public float MaxVelocityToMerge = 5f;

    [Header("Merge Visualization")]
    public float VisualRadiusMultiplier = 0.8f;
    public float PositionCorrectionFactor = 1.2f;
}