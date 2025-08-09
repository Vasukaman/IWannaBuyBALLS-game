// Filename: MagnetProfile.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewMagnetProfile", menuName = "Profiles/Physics/Magnet Profile")]
public class MagnetProfile : ScriptableObject
{
    [Header("Magnetism Settings")]
    [Tooltip("The local position within the parent that the Rigidbody2D will be magnetized to.")]
    public Vector2 TargetLocalPosition = Vector2.zero;
    public float MinMagnetForce = 10f;
    public float MinForceDistance = 0.1f;
    public float MaxMagnetForce = 100f;
    public float MaxForceDistance = 5f;

    [Header("Outer Limit Settings")]
    public float MaxAllowedDistance = 6f;
    public float LimitRampUpDistance = 1f;
    public float MaxLimitForceMagnitude = 200f;
    [Range(1f, 10f)]
    public float LimitForcePower = 2f;

    [Header("Settling Control")]
    [Range(0f, 1f)]
    public float DampingFactor = 0.95f;
    public float SnapDistance = 0.05f;
    public bool ZeroVelocityOnSnap = true;

    [Header("Gizmo Settings")]
    public bool DrawGizmos = true;
    public Color GizmoColor = Color.yellow;
    public float GizmoRadius = 0.1f;
    public Color LimitGizmoColor = Color.red;
    public Color SnapGizmoColor = Color.green;
}