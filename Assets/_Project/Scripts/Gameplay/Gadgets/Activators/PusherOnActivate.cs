// Filename: PusherOnActivate.cs
using Gameplay.Interfaces;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A "Gadget Behaviour" that applies a random impulse force to a Rigidbody2D
    /// when it receives a signal from an IActivationSource, configured by a PusherProfile.
    /// </summary>
    public class PusherOnActivate : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The ScriptableObject asset that defines the behavior of this pusher.")]
        [SerializeField] private PusherProfile _profile;

        [Header("Dependencies")]
        [Tooltip("Assign any component that implements IActivationSource.")]
        [SerializeField] private MonoBehaviour _activationSource;

        [Tooltip("The Rigidbody2D to push. If null, it will search on this GameObject.")]
        [SerializeField] private Rigidbody2D _bodyToPush;

        // --- State ---
        private IActivationSource _source;

        // --- Unity Methods ---

        private void Awake()
        {
            if (_profile == null)
            {
                Debug.LogError("PusherOnActivate is missing a PusherProfile! Disabling component.", this);
                enabled = false;
                return;
            }

            if (_bodyToPush == null)
            {
                _bodyToPush = GetComponent<Rigidbody2D>();
            }

            _source = _activationSource as IActivationSource;
            if (_source == null && _activationSource != null)
            {
                Debug.LogWarning("PusherOnActivate: The assigned Activation Source does not implement IActivationSource.", this);
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
            // The logic now reads the force value from the profile.
            Vector2 impulse = randomDirection * _profile.MaxImpulse;
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