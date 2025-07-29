// Filename: ForceZone.cs
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A gadget that applies a constant force to any Rigidbody2D on specified layers
    /// that enters its trigger volume.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ForceZone : MonoBehaviour
    {
        [Header("Force Settings")]
        [Tooltip("The layers that this force zone will affect.")]
        [SerializeField] private LayerMask _targetLayers;

        [Tooltip("The world-space direction the force will be applied in. Will be normalized.")]
        [SerializeField] private Vector2 _forceDirection = Vector2.up;

        [Tooltip("The strength of the force to apply.")]
        [SerializeField] private float _forceMagnitude = 10f;

        [Tooltip("The type of force to apply. 'Force' uses mass, 'Acceleration' ignores mass.")]
        [SerializeField] private ForceMode2D _forceMode = ForceMode2D.Force;

        // --- Cached Data ---
        private Vector2 _forceVector;

        // --- Unity Methods ---

        private void Awake()
        {
            // Ensure the collider attached to this object is configured as a trigger.
            // This is necessary for OnTriggerStay2D to be called.
            var triggerCollider = GetComponent<Collider2D>();
            if (!triggerCollider.isTrigger)
            {
                triggerCollider.isTrigger = true;
            }

            // Cache the calculated force vector to avoid recalculating the normalization
            // and multiplication on every physics update for every object in the trigger.
            _forceVector = _forceDirection.normalized * _forceMagnitude;
        }

        /// <summary>
        /// Called by the Unity physics engine for every frame a Collider2D remains within this trigger.
        /// </summary>
        private void OnTriggerStay2D(Collider2D other)
        {
            // Check if the other object's layer is included in our target layers mask.
            // This is an efficient bitwise operation to see if the layer flag is set.
            if ((_targetLayers.value & (1 << other.gameObject.layer)) > 0)
            {
                // If the layer matches, attempt to get the Rigidbody2D component from the other object.
                // Using TryGetComponent is safe and avoids null exceptions if no rigidbody is present.
                if (other.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    // Apply the cached force vector to the rigidbody.
                    rb.AddForce(_forceVector, _forceMode);
                }
            }
        }
    }
}