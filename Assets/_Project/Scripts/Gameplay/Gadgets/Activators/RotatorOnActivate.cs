// Filename: RotatorOnActivate.cs
using Gameplay.Interfaces;
using System.Collections;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A "Gadget Behaviour" that smoothly rotates a target GameObject when it receives
    /// a signal from an IActivationSource, configured by a RotatorProfile.
    /// </summary>
    public class RotatorOnActivate : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The ScriptableObject asset that defines the behavior of this rotator.")]
        [SerializeField] private RotatorProfile _profile;

        [Header("Dependencies")]
        [Tooltip("Assign any component that implements IActivationSource.")]
        [SerializeField] private MonoBehaviour _activationSource;

        [Tooltip("The GameObject to rotate. If null, this GameObject will be rotated.")]
        [SerializeField] private GameObject _objectToRotate;

        // --- State ---
        private IActivationSource _source;
        private Quaternion _targetRotation;
        private bool _isRotating = false;
        private Transform _cachedTransformToRotate;

        // --- Unity Methods ---

        private void Awake()
        {
            if (_profile == null)
            {
                Debug.LogError("RotatorOnActivate is missing a RotatorProfile! Disabling component.", this);
                enabled = false;
                return;
            }

            if (_objectToRotate == null)
            {
                _objectToRotate = this.gameObject;
            }
            _cachedTransformToRotate = _objectToRotate.transform;
            _targetRotation = _cachedTransformToRotate.rotation;

            _source = _activationSource as IActivationSource;
            if (_source == null && _activationSource != null)
            {
                Debug.LogWarning("RotatorOnActivate: The assigned Activation Source does not implement IActivationSource.", this);
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
            if (_isRotating) return;

            // The logic now reads the rotation amount from the profile.
            _targetRotation *= Quaternion.Euler(0, 0, _profile.RotationDegreesPerActivate);
            StartCoroutine(SmoothRotateCoroutine(_targetRotation));
        }

        private IEnumerator SmoothRotateCoroutine(Quaternion endRotation)
        {
            _isRotating = true;
            float timer = 0f;
            Quaternion startRotation = _cachedTransformToRotate.rotation;

            // The logic now reads the duration from the profile.
            while (timer < _profile.RotationDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / _profile.RotationDuration);

                // The logic now reads the easing curve from the profile.
                float easedProgress = _profile.RotationCurve.Evaluate(progress);

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