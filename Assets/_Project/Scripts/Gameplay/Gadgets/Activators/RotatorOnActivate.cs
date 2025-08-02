// Filename: RotatorOnActivate.cs
using Gameplay.Interfaces; // Use the new interface namespace
using System.Collections;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A "Gadget Behaviour" that smoothly rotates a target GameObject by a specified amount
    /// when it receives a signal from any IActivationSource.
    /// </summary>
    public class RotatorOnActivate : MonoBehaviour
    {
        [Header("Dependencies")]
        [Tooltip("Assign any component that implements IActivationSource (e.g., ManualActivatorController or AutoActivatorController).")]
        [SerializeField] private MonoBehaviour _activationSource;

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
        private IActivationSource _source;
        private Quaternion _targetRotation;
        private bool _isRotating = false;
        private Transform _cachedTransformToRotate;

        // --- Unity Methods ---

        private void Awake()
        {
            if (_objectToRotate == null)
            {
                _objectToRotate = this.gameObject;
            }
            _cachedTransformToRotate = _objectToRotate.transform;
            _targetRotation = _cachedTransformToRotate.rotation;
            
            // Safely get the interface from the assigned MonoBehaviour.
            _source = _activationSource as IActivationSource;
            if (_source == null && _activationSource != null)
            {
                Debug.LogWarning("RotatorOnActivate: The assigned Activation Source does not implement the IActivationSource interface.", this);
            }
        }

        private void OnEnable()
        {
            if (_source != null)
            {
                _source.OnActivate += HandleActivation;
            }
            else
            {
                Debug.LogWarning("RotatorOnActivate: Activation Source not assigned. Rotations will not be triggered.", this);
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
            if (_isRotating) return;

            _targetRotation *= Quaternion.Euler(0, 0, _rotationDegreesPerActivate);
            StartCoroutine(SmoothRotateCoroutine(_targetRotation));
        }

        private IEnumerator SmoothRotateCoroutine(Quaternion endRotation)
        {
            _isRotating = true;
            float timer = 0f;
            Quaternion startRotation = _cachedTransformToRotate.rotation;

            while (timer < _rotationDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / _rotationDuration);
                float easedProgress = _rotationCurve.Evaluate(progress);

                _cachedTransformToRotate.rotation = Quaternion.Slerp(startRotation, endRotation, easedProgress);
                
                yield return null;
            }

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
                
                Gizmos.color = Color.red;
                Gizmos.DrawLine(targetTransform.position, targetTransform.position + targetTransform.right * 0.5f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(targetTransform.position, targetTransform.position + targetTransform.up * 0.5f);
            }
        }
    }
}