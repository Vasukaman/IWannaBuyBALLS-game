// Filename: RotatorProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewRotatorProfile", menuName = "Profiles/Gadgets/Rotator Profile")]
public class RotatorProfile : ScriptableObject
{
    [Header("Rotation Settings")]
    [Tooltip("Degrees to rotate each time the activator is triggered.")]
    [Range(1f, 360f)]
    public float RotationDegreesPerActivate = 90f;

    [Tooltip("Time in seconds for the rotation to complete smoothly.")]
    public float RotationDuration = 0.5f;

    [Tooltip("An animation curve to control the easing of the rotation.")]
    public AnimationCurve RotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
}