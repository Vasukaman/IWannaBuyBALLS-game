// Filename: LocalPointMagnet.cs
using UnityEngine;

namespace Core.Physics
{
    /// <summary>
    /// A physics component that attracts a Rigidbody2D towards a target point defined in the
    /// local space of its parent. It includes logic for variable force, boundary limits, and damping.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class LocalPointMagnet : MonoBehaviour
    {
        [Header("Magnetism Settings")]
        [Tooltip("The local position within this object's parent that the Rigidbody2D will be magnetized to.")]
        [SerializeField] private Vector2 _targetLocalPosition = Vector2.zero;
        [SerializeField] private float _minMagnetForce = 10f;
        [SerializeField] private float _minForceDistance = 0.1f;
        [SerializeField] private float _maxMagnetForce = 100f;
        [SerializeField] private float _maxForceDistance = 5f;

        [Header("Outer Limit Settings")]
        [Tooltip("The maximum distance the Rigidbody2D can move away from the target point.")]
        [SerializeField] private float _maxAllowedDistance = 6f;
        [Tooltip("How many units *before* maxAllowedDistance the repulsive force starts to ramp up.")]
        [SerializeField] private float _limitRampUpDistance = 1f;
        [Tooltip("The maximum additional force applied at the maxAllowedDistance.")]
        [SerializeField] private float _maxLimitForceMagnitude = 200f;
        [Range(1f, 10f)]
        [SerializeField] private float _limitForcePower = 2f;

        [Header("Settling Control")]
        [Tooltip("Damping factor to reduce oscillations. Higher values make it stop faster.")]
        [Range(0f, 1f)]
        [SerializeField] private float _dampingFactor = 0.95f;
        [Tooltip("If the Rigidbody2D is closer than this distance, it will snap into place.")]
        [SerializeField] private float _snapDistance = 0.05f;
        [Tooltip("If true, velocity will be zeroed when within snapDistance.")]
        [SerializeField] private bool _zeroVelocityOnSnap = true;

        [Header("Gizmo Settings")]
        [SerializeField] private bool _drawGizmos = true;
        [SerializeField] private Color _gizmoColor = Color.yellow;
        [SerializeField] private float _gizmoRadius = 0.1f;
        [SerializeField] private Color _limitGizmoColor = Color.red;
        [SerializeField] private Color _snapGizmoColor = Color.green;

        // --- State & Cache ---
        private Rigidbody2D _rigidbody;

        // --- Unity Methods ---

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            Vector2 targetWorldPosition = GetTargetWorldPosition();
            Vector2 directionToTarget = targetWorldPosition - _rigidbody.position;
            float distance = directionToTarget.magnitude;

            // Handle snapping if the object is very close to the target.
            if (HandleSnapping(targetWorldPosition, distance))
            {
                return; // Object is snapped, no further physics needed.
            }

            // Calculate forces
            float magnetismForce = CalculateMagnetismForce(distance);
            float boundaryForce = CalculateBoundaryForce(distance);
            float totalForceMagnitude = magnetismForce + boundaryForce;

            // Apply forces and damping
            _rigidbody.AddForce(directionToTarget.normalized * totalForceMagnitude, ForceMode2D.Force);
            _rigidbody.velocity *= _dampingFactor;
        }

        // --- Private Logic ---

        /// <summary>
        /// If the object is within the snap distance, lock its position and velocity.
        /// </summary>
        /// <returns>True if the object was snapped, false otherwise.</returns>
        private bool HandleSnapping(Vector2 targetWorldPosition, float distance)
        {
            if (distance <= _snapDistance)
            {
                _rigidbody.position = targetWorldPosition;
                if (_zeroVelocityOnSnap)
                {
                    _rigidbody.velocity = Vector2.zero;
                    _rigidbody.angularVelocity = 0f;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Calculates the primary attractive force based on distance.
        /// </summary>
        private float CalculateMagnetismForce(float distance)
        {
            if (distance <= _minForceDistance)
            {
                return _minMagnetForce;
            }
            if (distance < _maxForceDistance)
            {
                float progress = Mathf.InverseLerp(_minForceDistance, _maxForceDistance, distance);
                return Mathf.Lerp(_minMagnetForce, _maxMagnetForce, progress);
            }
            return _maxMagnetForce;
        }

        /// <summary>
        /// Calculates the strong repulsive force when the object nears its maximum allowed distance.
        /// </summary>
        private float CalculateBoundaryForce(float distance)
        {
            float limitRampUpStartPoint = _maxAllowedDistance - _limitRampUpDistance;
            if (distance <= limitRampUpStartPoint)
            {
                return 0f;
            }

            if (distance >= _maxAllowedDistance)
            {
                // If we've passed the boundary, apply an even stronger force.
                float overshoot = distance - _maxAllowedDistance;
                return _maxLimitForceMagnitude + (overshoot * _maxLimitForceMagnitude * 5f);
            }
            else
            {
                // We are within the ramp-up zone.
                float rampProgress = Mathf.InverseLerp(limitRampUpStartPoint, _maxAllowedDistance, distance);
                return Mathf.Pow(rampProgress, _limitForcePower) * _maxLimitForceMagnitude;
            }
        }

        /// <summary>
        /// Calculates the target's position in world space.
        /// </summary>
        private Vector2 GetTargetWorldPosition()
        {
            // TODO: [Architecture] This component's logic is tightly coupled to its parent's transform.
            // This is a common and acceptable Unity pattern, but it can be fragile. If this object is ever
            // unparented at runtime, its behavior will change.
            if (transform.parent != null)
            {
                return transform.parent.TransformPoint(_targetLocalPosition);
            }
            return transform.TransformPoint(_targetLocalPosition);
        }

        // --- Editor-Only Methods ---

        private void OnValidate()
        {
            // Ensures that the distance values make logical sense in the editor.
            _minForceDistance = Mathf.Max(0, _minForceDistance);
            _maxForceDistance = Mathf.Max(_minForceDistance + 0.01f, _maxForceDistance);
            _maxAllowedDistance = Mathf.Max(_maxForceDistance + 0.01f, _maxAllowedDistance);
            _limitRampUpDistance = Mathf.Clamp(_limitRampUpDistance, 0.01f, _maxAllowedDistance - _maxForceDistance);
            _snapDistance = Mathf.Max(0, _snapDistance);
        }

        private void OnDrawGizmos()
        {
            if (!_drawGizmos) return;

            Vector2 targetWorldPosition = GetTargetWorldPosition();

            // Draw snap distance
            Gizmos.color = _snapGizmoColor;
            Gizmos.DrawWireSphere(targetWorldPosition, _snapDistance);

            // Draw target point and line to object
            Gizmos.color = _gizmoColor;
            Gizmos.DrawSphere(targetWorldPosition, _gizmoRadius);
            if (_rigidbody != null)
            {
                Gizmos.DrawLine(_rigidbody.position, targetWorldPosition);
            }

            // Draw outer limit and ramp-up zone
            Gizmos.color = _limitGizmoColor;
            Gizmos.DrawWireSphere(targetWorldPosition, _maxAllowedDistance);
            Gizmos.color = new Color(_limitGizmoColor.r, _limitGizmoColor.g, _limitGizmoColor.b, 0.5f);
            Gizmos.DrawWireSphere(targetWorldPosition, _maxAllowedDistance - _limitRampUpDistance);
        }
    }
}