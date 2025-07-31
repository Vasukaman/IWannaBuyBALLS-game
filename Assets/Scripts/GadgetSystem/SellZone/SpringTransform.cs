// Filename: SpringTransform.cs
using UnityEngine;

namespace Core.Animation
{
    public class SpringTransform : MonoBehaviour
    {
        [Header("Spring Settings")]
        [SerializeField] private float _stiffness = 20f;
        [Range(0, 1)][SerializeField] private float _damping = 0.7f;

        // Public properties for the Presenter to control
        public Vector3 TargetPosition { get; set; }
        public Vector3 TargetScale { get; set; }

        private Vector3 _positionVelocity;
        private Vector3 _scaleVelocity;

        private void Awake()
        {
            // Initialize targets to the starting state
            TargetPosition = transform.position;
            TargetScale = transform.localScale;
        }

        private void Update()
        {
            // Animate position
            Vector3 positionForce = (TargetPosition - transform.position) * _stiffness;
            _positionVelocity = (_positionVelocity + positionForce * Time.deltaTime) * (1f - _damping);
            transform.position += _positionVelocity * Time.deltaTime;

            // Animate scale
            Vector3 scaleForce = (TargetScale - transform.localScale) * _stiffness;
            _scaleVelocity = (_scaleVelocity + scaleForce * Time.deltaTime) * (1f - _damping);
            transform.localScale += _scaleVelocity * Time.deltaTime;
        }
    }
}