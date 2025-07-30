// Filename: PusherOnActivate.cs
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    // A component that applies a random impulse force to a Rigidbody2D
    // when a specified activator's OnActivate event is triggered.
    /// </summary>
    public class PusherOnActivate : MonoBehaviour
    {
        [Header("Dependencies")]
        // TODO: [Coupling] This component is tightly coupled to the 'ManualActivator' class.
        // A more flexible design would be to depend on an interface (e.g., IActivatable),
        // allowing this pusher to be triggered by any type of activator.
        [Tooltip("The activator that will trigger the push.")]
        [SerializeField] private ManualActivator _activator;

        [Tooltip("The Rigidbody2D to push. If null, it will search on this GameObject.")]
        [SerializeField] private Rigidbody2D _bodyToPush;

        [Header("Push Settings")]
        [Tooltip("The maximum force applied as a single impulse.")]
        [Range(0.1f, 50f)]
        [SerializeField] private float _maxImpulse = 5f;

        // --- Unity Methods ---

        private void Awake()
        {
            // If a Rigidbody2D is not assigned in the inspector, try to find one on this object.
            if (_bodyToPush == null)
            {
                _bodyToPush = GetComponent<Rigidbody2D>();
            }

            if (_bodyToPush == null)
            {
                Debug.LogWarning("PusherOnActivate: Rigidbody2D not found or assigned.", this);
            }
        }

        private void OnEnable()
        {
            // Subscribe to the activator's event when this component is enabled.
            if (_activator != null)
            {
                _activator.OnActivate += HandleActivation;
            }
            else
            {
                Debug.LogWarning("PusherOnActivate: Activator not assigned. Push will not be triggered.", this);
            }
        }

        private void OnDisable()
        {
            // Unsubscribe from the event to prevent errors and memory leaks.
            if (_activator != null)
            {
                _activator.OnActivate -= HandleActivation;
            }
        }

        // --- Private Methods ---

        /// <summary>
        /// This method is called when the assigned activator's OnActivate event is invoked.
        /// </summary>
        private void HandleActivation()
        {
            if (_bodyToPush == null)
            {
                return;
            }

            // Apply a random directional force as an instant impulse.
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector2 impulse = randomDirection * _maxImpulse;
            _bodyToPush.AddForce(impulse, ForceMode2D.Impulse);
        }

        // --- Editor-Only Methods ---

        /// <summary>
        /// Draws a gizmo in the editor to visualize the connection to the Rigidbody2D that will be pushed.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_bodyToPush != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f); // Orange for visibility
                Gizmos.DrawWireSphere(_bodyToPush.transform.position, 0.5f);
                Gizmos.DrawLine(transform.position, _bodyToPush.transform.position);
            }
        }
    }
}