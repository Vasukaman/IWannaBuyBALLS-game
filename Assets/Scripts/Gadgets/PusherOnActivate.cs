// Filename: PusherOnActivate.cs
using Gameplay.Interfaces; // Use the new interface namespace
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A "Gadget Behaviour" that applies a random impulse force to a Rigidbody2D
    /// when it receives a signal from any IActivationSource.
    /// </summary>
    public class PusherOnActivate : MonoBehaviour
    {
        [Header("Dependencies")]
        // The dependency is now the flexible interface, not a concrete class.
        [Tooltip("Assign any component that implements IActivationSource (e.g., ManualActivatorController or AutoActivatorController).")]
        [SerializeField] private MonoBehaviour _activationSource;

        [Tooltip("The Rigidbody2D to push. If null, it will search on this GameObject.")]
        [SerializeField] private Rigidbody2D _bodyToPush;

        [Header("Push Settings")]
        [Tooltip("The maximum force applied as a single impulse.")]
        [Range(0.1f, 50f)]
        [SerializeField] private float _maxImpulse = 5f;

        // --- State ---
        private IActivationSource _source;

        // --- Unity Methods ---

        private void Awake()
        {
            if (_bodyToPush == null)
            {
                _bodyToPush = GetComponent<Rigidbody2D>();
            }

            // We get the interface from the assigned MonoBehaviour.
            // This is a safe way to handle interface fields in the Inspector.
            _source = _activationSource as IActivationSource;

            if (_source == null)
            {
                Debug.LogWarning("PusherOnActivate: The assigned Activation Source does not implement the IActivationSource interface.", this);
            }
        }

        private void OnEnable()
        {
            if (_source != null)
            {
                _source.OnActivate += HandleActivation;
            }
        }

        private void OnDisable()
        {
            if (_source != null)
            {
                _source.OnActivate -= HandleActivation;
            }
        }

        // --- Private Methods ---

        private void HandleActivation()
        {
            if (_bodyToPush == null) return;

            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            Vector2 impulse = randomDirection * _maxImpulse;
            _bodyToPush.AddForce(impulse, ForceMode2D.Impulse);
        }

        // --- Editor-Only Methods ---

        private void OnDrawGizmosSelected()
        {
            if (_bodyToPush != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f);
                Gizmos.DrawWireSphere(_bodyToPush.transform.position, 0.5f);
                Gizmos.DrawLine(transform.position, _bodyToPush.transform.position);
            }
        }
    }
}