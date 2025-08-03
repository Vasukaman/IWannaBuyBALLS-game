// Filename: LocalPointMagnet.cs
using UnityEngine;

namespace Core.Physics
{
    /// <summary>
    /// A physics component that attracts a Rigidbody2D towards a target point defined in a
    /// MagnetProfile. It includes logic for variable force, boundary limits, and damping.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class LocalPointMagnet : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The ScriptableObject asset that defines the behavior of this magnet.")]
        [SerializeField] private MagnetProfile _profile;

        // --- State & Cache ---
        private Rigidbody2D _rigidbody;

        // --- Unity Methods ---

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            if (_profile == null)
            {
                Debug.LogError("LocalPointMagnet is missing a MagnetProfile! Disabling component.", this);
                enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (_profile == null) return;

            Vector2 targetWorldPosition = GetTargetWorldPosition();
            Vector2 directionToTarget = targetWorldPosition - _rigidbody.position;
            float distance = directionToTarget.magnitude;

            if (HandleSnapping(targetWorldPosition, distance))
            {
                return;
            }

            float magnetismForce = CalculateMagnetismForce(distance);
            float boundaryForce = CalculateBoundaryForce(distance);
            float totalForceMagnitude = magnetismForce + boundaryForce;

            _rigidbody.AddForce(directionToTarget.normalized * totalForceMagnitude, ForceMode2D.Force);
            _rigidbody.velocity *= _profile.DampingFactor;
        }

        // --- Private Logic ---

        private bool HandleSnapping(Vector2 targetWorldPosition, float distance)
        {
            if (distance <= _profile.SnapDistance)
            {
                _rigidbody.position = targetWorldPosition;
                if (_profile.ZeroVelocityOnSnap)
                {
                    _rigidbody.velocity = Vector2.zero;
                    _rigidbody.angularVelocity = 0f;
                }
                return true;
            }
            return false;
        }

        private float CalculateMagnetismForce(float distance)
        {
            if (distance <= _profile.MinForceDistance) return _profile.MinMagnetForce;
            if (distance < _profile.MaxForceDistance)
            {
                float progress = Mathf.InverseLerp(_profile.MinForceDistance, _profile.MaxForceDistance, distance);
                return Mathf.Lerp(_profile.MinMagnetForce, _profile.MaxMagnetForce, progress);
            }
            return _profile.MaxMagnetForce;
        }

        private float CalculateBoundaryForce(float distance)
        {
            float rampStart = _profile.MaxAllowedDistance - _profile.LimitRampUpDistance;
            if (distance <= rampStart) return 0f;

            if (distance >= _profile.MaxAllowedDistance)
            {
                float overshoot = distance - _profile.MaxAllowedDistance;
                return _profile.MaxLimitForceMagnitude + (overshoot * _profile.MaxLimitForceMagnitude * 5f);
            }
            else
            {
                float rampProgress = Mathf.InverseLerp(rampStart, _profile.MaxAllowedDistance, distance);
                return Mathf.Pow(rampProgress, _profile.LimitForcePower) * _profile.MaxLimitForceMagnitude;
            }
        }

        private Vector2 GetTargetWorldPosition()
        {
            if (transform.parent != null)
            {
                return transform.parent.TransformPoint(_profile.TargetLocalPosition);
            }
            return transform.TransformPoint(_profile.TargetLocalPosition);
        }

        // --- Editor-Only Methods ---

        private void OnDrawGizmos()
        {
            if (_profile == null || !_profile.DrawGizmos) return;

            Vector2 targetWorldPosition = GetTargetWorldPosition();

            Gizmos.color = _profile.SnapGizmoColor;
            Gizmos.DrawWireSphere(targetWorldPosition, _profile.SnapDistance);

            Gizmos.color = _profile.GizmoColor;
            Gizmos.DrawSphere(targetWorldPosition, _profile.GizmoRadius);
            if (_rigidbody != null)
            {
                Gizmos.DrawLine(_rigidbody.position, targetWorldPosition);
            }

            Gizmos.color = _profile.LimitGizmoColor;
            Gizmos.DrawWireSphere(targetWorldPosition, _profile.MaxAllowedDistance);
            Gizmos.color = new Color(_profile.LimitGizmoColor.r, _profile.LimitGizmoColor.g, _profile.LimitGizmoColor.b, 0.5f);
            Gizmos.DrawWireSphere(targetWorldPosition, _profile.MaxAllowedDistance - _profile.LimitRampUpDistance);
        }
    }
}