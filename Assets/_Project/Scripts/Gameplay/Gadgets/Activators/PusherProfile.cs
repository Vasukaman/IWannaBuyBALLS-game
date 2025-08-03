// Filename: PusherProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewPusherProfile", menuName = "Profiles/Physics/Pusher Profile")]
public class PusherProfile : ScriptableObject
{
    [Header("Push Settings")]
    [Tooltip("The maximum force applied as a single impulse.")]
    [Range(0.1f, 50f)]
    public float MaxImpulse = 5f;
}