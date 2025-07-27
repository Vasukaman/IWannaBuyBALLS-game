using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MagnetToLocalPoint2D : MonoBehaviour
{
    [Tooltip("The Rigidbody2D to magnetize.")]
    private Rigidbody2D rb2d;

    [Header("Magnetism Settings")]
    [Tooltip("The local position within this GameObject's parent that the Rigidbody2D will be magnetized to.")]
    public Vector2 targetLocalPosition = Vector2.zero;

    [Tooltip("The minimum force applied when the Rigidbody2D is very close to the target.")]
    public float minMagnetForce = 10f;
    [Tooltip("The distance at which the minimum magnetism force applies (or closer).")]
    public float minForceDistance = 0.1f;

    [Tooltip("The maximum force applied when the Rigidbody2D is far from the target (within normal limits).")]
    public float maxMagnetForce = 100f;
    [Tooltip("The distance at which the maximum magnetism force applies (before the outer limit kicks in).")]
    public float maxForceDistance = 5f;

    [Header("Outer Limit Settings")]
    [Tooltip("The maximum distance the Rigidbody2D can move away from the target point.")]
    public float maxAllowedDistance = 6f;
    [Tooltip("How many units *before* maxAllowedDistance the repulsive force starts to ramp up.")]
    public float limitRampUpDistance = 1f;
    [Tooltip("The maximum additional force applied at the maxAllowedDistance.")]
    public float maxLimitForceMagnitude = 200f;
    [Tooltip("Controls the curve of the limit force ramp-up. Higher values make it ramp up more sharply.")]
    [Range(1f, 10f)]
    public float limitForcePower = 2f;

    [Header("Wobble & Settling Control")]
    [Tooltip("Damping factor to reduce oscillations. Higher values make it stop faster.")]
    [Range(0f, 1f)]
    public float dampingFactor = 0.95f;

    [Tooltip("If the Rigidbody2D is closer than this distance to the target, it will snap into place and stop wobbling.")]
    public float snapDistance = 0.05f; // New parameter!
    [Tooltip("If true, velocity will be zeroed when within snapDistance, even if not snapping to position.")]
    public bool zeroVelocityOnSnap = true;


    [Header("Gizmo Settings")]
    [Tooltip("Optional: Visualizes the target local position and outer limit in the editor.")]
    public bool drawGizmo = true;
    [Tooltip("Gizmo color for the target point.")]
    public Color gizmoColor = Color.yellow;
    [Tooltip("Gizmo radius for the target point.")]
    public float gizmoRadius = 0.1f;
    [Tooltip("Gizmo color for the outer limit circle.")]
    public Color limitGizmoColor = Color.red;
    [Tooltip("Gizmo color for the snap distance circle.")]
    public Color snapGizmoColor = Color.green;


    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            Debug.LogError("MagnetToLocalPoint2D requires a Rigidbody2D component on the same GameObject!");
            enabled = false;
        }

        ValidateDistances();
    }

    void OnValidate()
    {
        ValidateDistances();
    }

    void ValidateDistances()
    {
        if (minForceDistance >= maxForceDistance)
        {
            Debug.LogWarning($"MagnetToLocalPoint2D on {gameObject.name}: minForceDistance ({minForceDistance}) should be less than maxForceDistance ({maxForceDistance}). Adjusting maxForceDistance.", this);
            maxForceDistance = minForceDistance + 0.1f;
        }
        if (maxForceDistance >= maxAllowedDistance)
        {
            Debug.LogWarning($"MagnetToLocalPoint2D on {gameObject.name}: maxForceDistance ({maxForceDistance}) should be less than maxAllowedDistance ({maxAllowedDistance}). Adjusting maxAllowedDistance.", this);
            maxAllowedDistance = maxForceDistance + 0.1f;
        }
        if (limitRampUpDistance <= 0)
        {
            Debug.LogWarning($"MagnetToLocalPoint2D on {gameObject.name}: limitRampUpDistance ({limitRampUpDistance}) should be greater than 0. Adjusting to 0.1.", this);
            limitRampUpDistance = 0.1f;
        }
        if (snapDistance < 0)
        {
            Debug.LogWarning($"MagnetToLocalPoint2D on {gameObject.name}: snapDistance ({snapDistance}) should be non-negative. Adjusting to 0.", this);
            snapDistance = 0f;
        }

        if ((maxAllowedDistance - limitRampUpDistance) < maxForceDistance)
        {
            Debug.LogWarning($"MagnetToLocalPoint2D on {gameObject.name}: Outer limit ramp-up starts before basic magnetism reaches its peak. This might create a less distinct transition. Consider adjusting maxForceDistance or limitRampUpDistance.", this);
        }
    }


    void FixedUpdate() // Use FixedUpdate for physics calculations with Rigidbody2D
    {
        Vector2 targetWorldPosition = (Vector2)(transform.parent != null ?
                                                  transform.parent.TransformPoint(targetLocalPosition) :
                                                  transform.TransformPoint(targetLocalPosition));

        Vector2 directionToTarget = targetWorldPosition - rb2d.position;
        float distance = directionToTarget.magnitude;

        // --- NEW: Wobble & Settling Logic ---
        if (distance <= snapDistance)
        {
            // Within snap distance: snap to target and stop motion
            rb2d.position = targetWorldPosition; // Directly set position
            if (zeroVelocityOnSnap)
            {
                rb2d.velocity = Vector2.zero; // Stop all movement
                rb2d.angularVelocity = 0f; // Stop rotation too
            }
            return; // No more forces needed, object is settled
        }

        Vector2 forceDirection = directionToTarget.normalized;
        float totalForceMagnitude = 0f;

        // --- 1. Calculate the primary magnetism force ---
        if (distance <= minForceDistance)
        {
            totalForceMagnitude = minMagnetForce;
        }
        else if (distance < maxForceDistance)
        {
            float normalizedDistance = Mathf.InverseLerp(minForceDistance, maxForceDistance, distance);
            totalForceMagnitude = Mathf.Lerp(minMagnetForce, maxMagnetForce, normalizedDistance);
        }
        else // distance >= maxForceDistance
        {
            totalForceMagnitude = maxMagnetForce;
        }


        // --- 2. Add the "outer boundary limit" force ---
        float limitRampUpStartPoint = maxAllowedDistance - limitRampUpDistance;

        if (distance > limitRampUpStartPoint)
        {
            float limitForceMagnitudeToAdd = 0f;

            if (distance >= maxAllowedDistance)
            {
                float overshoot = distance - maxAllowedDistance;
                limitForceMagnitudeToAdd = maxLimitForceMagnitude + (overshoot * maxLimitForceMagnitude * 5f);
            }
            else
            {
                float rampProgress = Mathf.InverseLerp(limitRampUpStartPoint, maxAllowedDistance, distance);
                limitForceMagnitudeToAdd = Mathf.Pow(rampProgress, limitForcePower) * maxLimitForceMagnitude;
            }
            totalForceMagnitude += limitForceMagnitudeToAdd;
        }

        // Apply the combined force
        rb2d.AddForce(forceDirection * totalForceMagnitude, ForceMode2D.Force);

        // Apply damping to reduce oscillation (this applies *before* snap if not within snapDistance)
        rb2d.velocity *= dampingFactor;
    }

    void OnDrawGizmos()
    {
        if (drawGizmo && rb2d != null)
        {
            Vector2 targetWorldPosition = (Vector2)(transform.parent != null ?
                                                      transform.parent.TransformPoint(targetLocalPosition) :
                                                      transform.TransformPoint(targetLocalPosition));

            // Draw target point
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(targetWorldPosition, gizmoRadius);
            Gizmos.DrawLine(rb2d.position, targetWorldPosition);

            // Draw outer limit circle
            Gizmos.color = limitGizmoColor;
            Gizmos.DrawWireSphere(targetWorldPosition, maxAllowedDistance);

            // Draw limit ramp-up start point
            Gizmos.color = new Color(limitGizmoColor.r, limitGizmoColor.g, limitGizmoColor.b, 0.5f);
            Gizmos.DrawWireSphere(targetWorldPosition, maxAllowedDistance - limitRampUpDistance);

            // NEW: Draw snap distance circle
            Gizmos.color = snapGizmoColor;
            Gizmos.DrawWireSphere(targetWorldPosition, snapDistance);
        }
    }
}