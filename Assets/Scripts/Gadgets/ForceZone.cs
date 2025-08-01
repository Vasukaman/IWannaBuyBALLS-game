// Filename: ForceZone.cs
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A "Gadget Behaviour" that applies a constant force to any Rigidbody2D on specified layers
    /// that remains within its trigger volume.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ForceZone : MonoBehaviour
    {
        [Header("Force Settings")]
        [Tooltip("The physics layers that this force zone will affect.")]
        [SerializeField] private LayerMask _targetLayers;

        [Tooltip("The world-space direction the force will be applied in. This will be normalized.")]
        [SerializeField] private Vector2 _forceDirection = Vector2.up;

        [Tooltip("The strength of the force to apply.")]
        [SerializeField] private float _forceMagnitude = 10f;

        [Tooltip("The type of force to apply. 'Force' uses mass, 'Acceleration' ignores mass.")]
        [SerializeField] private ForceMode2D _forceMode = ForceMode2D.Force;

        // --- Cached Data ---
        private Vector2 _cachedForceVector;

        // --- Unity Methods ---

        private void Awake()
        {
            // Ensure the collider is a trigger, which is required for OnTrigger events.
            if (TryGetComponent<Collider2D>(out var triggerCollider))
            {
                triggerCollider.isTrigger = true;
            }

            _cachedForceVector = _forceDirection.normalized * _forceMagnitude;
        }

        /// <summary>
        /// Called by the Unity physics engine for every frame a Collider2D remains within this trigger.
        /// </summary>
        private void OnTriggerStay2D(Collider2D other)
        {
            TryApplyForce(other);
        }

        // --- Private Methods ---

        /// <summary>
        /// Checks if the provided collider is a valid target and, if so, applies the cached force to it.
        /// </summary>
        private void TryApplyForce(Collider2D other)
        {
            // First, perform an efficient bitwise check to see if the object's layer is in our target mask.
            // This is the fastest way to filter physics interactions.
            if ((_targetLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                return; // Not a target layer, do nothing.
            }

            // If the layer matches, attempt to get the Rigidbody2D component.
            // Using TryGetComponent is safe and avoids errors if the object doesn't have a rigidbody.
            if (other.TryGetComponent<Rigidbody2D>(out var rb))
            {
                // Apply the pre-calculated force vector.
                rb.AddForce(_cachedForceVector, _forceMode);
            }
        }
    }
}