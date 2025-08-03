// Filename: BallProximityProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewBallProximityProfile", menuName = "Profiles/Ball/Proximity Profile")]
public class BallProximityProfile : ScriptableObject
{
    [Header("Proximity Settings")]
    public float ProximityRadius = 2f;
    public int MaxNeighbors = 4;
}