// Filename: ForceZone.cs
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A "Gadget Behaviour" that applies a constant force to any Rigidbody2D,
    /// configured by a ForceZoneProfile.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ForceZone : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The ScriptableObject asset that defines the behavior of this force zone.")]
        [SerializeField] private ForceZoneProfile _profile;

        // --- Cached Data ---
        private Vector2 _cachedForceVector;

        // --- Unity Methods ---

        private void Awake()
        {
            if (_profile == null)
            {
                Debug.LogError("ForceZone is missing a ForceZoneProfile! Disabling component.", this);
                enabled = false;
                return;
            }

            if (TryGetComponent<Collider2D>(out var triggerCollider))
            {
                triggerCollider.isTrigger = true;
            }

            _cachedForceVector = _profile.ForceDirection.normalized * _profile.ForceMagnitude;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // The logic remains the same, but it reads from the profile.
            if ((_profile.TargetLayers.value & (1 << other.gameObject.layer)) > 0)
            {
                if (other.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.AddForce(_cachedForceVector, _profile.ForceMode);
                }
            }
        }
    }
}