// Filename: SpringTransform.cs
using UnityEngine;

namespace Core.Animation
{
    public class SpringTransform : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SpringTransformProfile _profile;

        public Vector3 TargetPosition { get; set; }
        public Vector3 TargetScale { get; set; }

        private Vector3 _positionVelocity;
        private Vector3 _scaleVelocity;

        private void Awake()
        {
            if (_profile == null)
            {
                Debug.LogError("SpringTransform is missing a Profile! Disabling component.", this);
                enabled = false;
                return;
            }
            TargetPosition = transform.position;
            TargetScale = transform.localScale;
        }

        private void Update()
        {
            // Logic now uses _profile.Stiffness and _profile.Damping
            Vector3 positionForce = (TargetPosition - transform.position) * _profile.Stiffness;
            _positionVelocity = (_positionVelocity + positionForce * Time.deltaTime) * (1f - _profile.Damping);
            transform.position += _positionVelocity * Time.deltaTime;

            Vector3 scaleForce = (TargetScale - transform.localScale) * _profile.Stiffness;
            _scaleVelocity = (_scaleVelocity + scaleForce * Time.deltaTime) * (1f - _profile.Damping);
            transform.localScale += _scaleVelocity * Time.deltaTime;
        }
    }
}