// Filename: AutoActivator.cs
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Gameplay.Gadgets
{
    /// <summary>
    /// A gadget that automatically finds and activates the nearest IActivatable object at a regular interval.
    /// </summary>
    public class AutoActivator : MonoBehaviour, ICanConnect
    {
        [Header("Activation Settings")]
        [Tooltip("How often (in seconds) to activate the target.")]
        [SerializeField] private float _activationInterval = 1f;

        [Tooltip("How often (in seconds) to rescan for the nearest target.")]
        [SerializeField] private float _targetScanInterval = 2.0f;

        // --- State ---
        private IActivatable _currentTarget;

        // --- ICanConnect Implementation ---
        public event Action OnActivate;
        public Transform GetStartTransform => this.transform;
        public Transform GetTargetTransform => _currentTarget?.ActivationTransform;

        // --- Unity Methods ---

        private void Start()
        {
            StartCoroutine(ActivationRoutine());
        }

        // --- Private Logic & Coroutines ---

        /// <summary>
        /// The main coroutine that orchestrates finding and activating targets.
        /// </summary>
        private IEnumerator ActivationRoutine()
        {
            // Use WaitForSeconds objects to avoid generating garbage in the loop.
            var activationWait = new WaitForSeconds(_activationInterval);
            var targetScanWait = new WaitForSeconds(_targetScanInterval);

            while (true)
            {
                // TODO: [Performance] This component searches the entire scene for targets. This is inefficient.
                // A better architecture would be a central registry or manager where IActivatable objects
                // register themselves, allowing this component to query a simple list instead of the scene.
                FindNearestTarget();

                if (_currentTarget != null)
                {
                    _currentTarget.Activate();
                    OnActivate?.Invoke();
                }

                // Wait for the next activation and scan cycle.
                // This is much more performant than running FindObjectsOfType in Update().
                yield return activationWait;
                yield return targetScanWait;
            }
        }

        /// <summary>
        /// Finds all IActivatable objects in the scene and sets the nearest one as the current target.
        /// </summary>
        private void FindNearestTarget()
        {
            // Find all MonoBehaviours that implement the IActivatable interface.
            var allActivatables = FindObjectsOfType<MonoBehaviour>().OfType<IActivatable>();

            IActivatable nearestTarget = null;
            float minDistance = float.MaxValue;

            // Iterate through them to find the one with the smallest distance.
            foreach (var activatable in allActivatables)
            {
                // Skip self if this object also happens to be IActivatable.
                if (activatable as MonoBehaviour == this) continue;

                float distance = Vector3.Distance(transform.position, activatable.ActivationTransform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTarget = activatable;
                }
            }

            _currentTarget = nearestTarget;
        }
    }
}