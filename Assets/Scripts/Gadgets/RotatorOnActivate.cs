// Filename: RotatorOnActivate.cs
using System.Collections;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A component that smoothly rotates a target GameObject by a specified amount
    /// when a linked activator's OnActivate event is triggered.
    /// </summary>
    public class RotatorOnActivate : MonoBehaviour
    {
        [Header("Dependencies")]
        // TODO: [Coupling] This component is tightly coupled to the 'ManualActivator' class.
        // A more flexible design would be to depend on an interface (e.g., IActivatable),
        // allowing this rotator to be triggered by any type of activator.
        [Tooltip("The activator that will trigger the rotation.")]
        [SerializeField] private ManualActivator _activator;

        [Tooltip("The GameObject to rotate. If null, this GameObject will be rotated.")]
        [SerializeField] private GameObject _objectToRotate;

        [Header("Rotation Settings")]
        [Tooltip("Degrees to rotate each time the activator is triggered.")]
        [Range(1f, 360f)]
        [SerializeField] private float _rotationDegreesPerActivate = 90f;

        [Tooltip("Time in seconds for the rotation to complete smoothly.")]
        [SerializeField] private float _rotationDuration = 0.5f;

        [Tooltip("An animation curve to control the easing of the rotation.")]
        [SerializeField] private AnimationCurve _rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // --- State ---
        private Quaternion _targetRotation;
        private bool _isRotating = false;
        private Transform _cachedTransformToRotate;

        // --- Unity Methods ---

        private void Awake()
        {
            // If objectToRotate is not assigned, default to this GameObject.
            if (_objectToRotate == null)
            {
                _objectToRotate = this.gameObject;
            }
            _cachedTransformToRotate = _objectToRotate.transform;

            // Initialize target rotation to the current rotation to prevent snapping on start.
            _targetRotation = _cachedTransformToRotate.rotation;
        }

        private void OnEnable()
        {
            // Subscribe to the activator's event.
            if (_activator != null)
            {
                _activator.OnActivate += HandleActivation;
            }
            else
            {
                Debug.LogWarning("RotatorOnActivate: Activator not assigned. Rotations will not be triggered.", this);
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
        /// Called when the assigned activator's OnActivate event is invoked.
        /// </summary>
        private void HandleActivation()
        {
            // Ignore new activations if a rotation is already in progress.
            if (_isRotating)
            {
                return;
            }

            // Calculate the new target rotation by applying the rotation increment.
            _targetRotation *= Quaternion.Euler(0, 0, _rotationDegreesPerActivate); // Rotates around the Z-axis for 2D.

            // Start the smooth rotation coroutine.
            StartCoroutine(SmoothRotateCoroutine(_targetRotation));
        }

        /// <summary>
        /// Smoothly animates the object from its current rotation to the target rotation over a set duration.
        /// </summary>
        private IEnumerator SmoothRotateCoroutine(Quaternion endRotation)
        {
            _isRotating = true;
            float timer = 0f;
            Quaternion startRotation = _cachedTransformToRotate.rotation;

            while (timer < _rotationDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / _rotationDuration);

                // Use the animation curve to get an eased progress value.
                float easedProgress = _rotationCurve.Evaluate(progress);

                // Spherically interpolate between the start and end rotations.
                _cachedTransformToRotate.rotation = Quaternion.Slerp(startRotation, endRotation, easedProgress);

                yield return null; // Wait for the next frame.
            }

            // Snap to the final rotation to ensure precision.
            _cachedTransformToRotate.rotation = endRotation;
            _isRotating = false;
        }

        // --- Editor-Only Methods ---

        private void OnDrawGizmosSelected()
        {
            if (_objectToRotate != null)
            {
                Transform targetTransform = _objectToRotate.transform;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(targetTransform.position, targetTransform.lossyScale * 1.1f);

                // Draw lines to represent the local axes.
                Gizmos.color = Color.red; // X-axis (right)
                Gizmos.DrawLine(targetTransform.position, targetTransform.position + targetTransform.right * 0.5f);
                Gizmos.color = Color.green; // Y-axis (up)
                Gizmos.DrawLine(targetTransform.position, targetTransform.position + targetTransform.up * 0.5f);
            }
        }
    }
}