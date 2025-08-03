// Filename: ForceZoneProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewForceZoneProfile", menuName = "Profiles/Physics/Force Zone Profile")]
public class ForceZoneProfile : ScriptableObject
{
    [Tooltip("The physics layers that this force zone will affect.")]
    public LayerMask TargetLayers;

    [Tooltip("The world-space direction the force will be applied in. This will be normalized.")]
    public Vector2 ForceDirection = Vector2.up;

    [Tooltip("The strength of the force to apply.")]
    public float ForceMagnitude = 10f;

    [Tooltip("The type of force to apply. 'Force' uses mass, 'Acceleration' ignores mass.")]
    public ForceMode2D ForceMode = ForceMode2D.Force;
}