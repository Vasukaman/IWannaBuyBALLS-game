// Filename: LiquidWobbleAnimator.cs
using Core.Physics;
using Gameplay.BallSystem;
using System.Collections.Generic;
using UnityEngine;
using VFX.Gadgets;

namespace Gameplay.Gadgets
{
    [RequireComponent(typeof(BallDetector), typeof(LiquidGateView))]
    public class LiquidWobbleAnimator : MonoBehaviour
    {
        [SerializeField] private static int MAX_BALLS = 20;
        [Header("Configuration")]
        [Tooltip("The data asset containing all animation and visual settings for this gate.")]
        [SerializeField] private LiquidGateProfile _profile; // The ONE place to assign this in the Inspector.

        // --- Component References ---
        private BallDetector _detector;
        private LiquidGateView _view;

        // --- State ---
        private float _currentSize, _currentPos, _velocity;
        private float _targetAngle, _currentAngle;
        private Vector3 _lastPosition;
        private readonly Vector4[] _ballDataArray = new Vector4[MAX_BALLS];
        private readonly Vector4[] _ballColorArray = new Vector4[MAX_BALLS];

        private void Awake()
        {
            _detector = GetComponent<BallDetector>();
            _view = GetComponent<LiquidGateView>();
            _lastPosition = transform.position;
            _currentPos = 0.5f;
            _view.SetProfile(_profile);
        }

        private void Update()
        {
            // Calculate world velocity
            Vector3 worldVelocity = (transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = transform.position;

            // Get data from the detector
            var detectedBalls = _detector.DetectedBalls;

            // Calculate all the complex logic
            HandleWobbleAndBulge(detectedBalls, worldVelocity);
            HandleTilt(worldVelocity);
            ResetToZeroWhenIdle(worldVelocity, detectedBalls.Count > 0);

            // Prepare data for the view
            int ballCount = PrepareBallDataForShader(detectedBalls);
            float noiseAmp = Mathf.Lerp(_profile.NoiseAmplitude, 0f, Mathf.Abs(_currentSize) / _profile.MaxBulge);

            // Tell the view to update the shader
            _view.UpdateShaderProperties(-_currentSize, _currentPos, _currentAngle, noiseAmp,
                ballCount, _ballDataArray, _ballColorArray);
        }

        private void HandleWobbleAndBulge(IReadOnlyList<BallView> balls, Vector3 worldVelocity)
        {
            float targetSize = 0f;
            float targetPos = 0.5f;
            float closestDistY = float.MaxValue;

            foreach (var ball in balls)
            {
                Vector3 localPos = transform.InverseTransformPoint(ball.transform.position);
                float distY = Mathf.Abs(localPos.y);
                if (distY < closestDistY)
                {
                    closestDistY = distY;
                    float uvRadius = ball.Radius / transform.lossyScale.x;
                    if (distY <= uvRadius * 1.5f)
                    {
                        targetSize = Mathf.Min(uvRadius * 2f, _profile.MaxBulge);
                        targetPos = localPos.x + 0.5f;
                    }
                }
            }

            Vector3 localVel = transform.InverseTransformDirection(worldVelocity);
            float tiltBulge = 0f;
            if (worldVelocity.magnitude > 0.01f)
            {
                tiltBulge = Mathf.Clamp(localVel.y * _profile.VelocitySensitivity, -_profile.MaxVelocityBulge, _profile.MaxVelocityBulge);
            }

            float combinedTarget = targetSize + tiltBulge;
            float force = (combinedTarget - _currentSize) * _profile.Spring;
            _velocity = (_velocity + force * Time.deltaTime) * Mathf.Exp(_profile.Damper * Time.deltaTime);
            _currentSize += _velocity * Time.deltaTime;
            _currentSize = Mathf.Clamp(_currentSize, -_profile.MaxBulge, _profile.MaxBulge);
            targetPos = Mathf.Clamp(targetPos, _profile.MinBulgePos, _profile.MaxBulgePos);
            _currentPos = Mathf.MoveTowards(_currentPos, targetPos, _profile.LerpSpeed * Time.deltaTime);
        }

        private void HandleTilt(Vector3 worldVelocity)
        {
            if (worldVelocity.magnitude > 0.01f)
            {
                Vector3 localVel = transform.InverseTransformDirection(worldVelocity);
                float rawAngle = Mathf.Atan2(localVel.y, -localVel.x) * Mathf.Rad2Deg;
                rawAngle = Mathf.Clamp(rawAngle, -_profile.MaxTiltAngle, _profile.MaxTiltAngle);
                rawAngle *= Mathf.Sign(_currentSize);
                _targetAngle = rawAngle;
            }
            else
            {
                _targetAngle = 0f;
            }
            _currentAngle = Mathf.LerpAngle(_currentAngle, _targetAngle, _profile.TiltLerpSpeed * Time.deltaTime);
        }

        private int PrepareBallDataForShader(IReadOnlyList<BallView> balls)
        {
            int count = Mathf.Min(balls.Count, MAX_BALLS);
            for (int i = 0; i < count; i++)
            {
                var ball = balls[i];
                Vector3 localPos = transform.InverseTransformPoint(ball.transform.position);
                Vector2 uvPos = new Vector2(localPos.x + 0.5f, localPos.y + 0.5f);
                float uvRadius = ball.Radius / transform.lossyScale.x;
                _ballDataArray[i] = new Vector4(uvPos.x, uvPos.y, uvRadius, 0);
                _ballColorArray[i] = ball.Color;
            }
            return count;
        }

        private void ResetToZeroWhenIdle(Vector3 worldVelocity, bool hasBalls)
        {
            const float epsilon = 0.001f;
            bool isMoving = worldVelocity.magnitude > epsilon;

            if (!isMoving && !hasBalls)
            {
                _currentSize = Mathf.Lerp(_currentSize, 0f, 5f * Time.deltaTime);
                _velocity = Mathf.Lerp(_velocity, 0f, 5f * Time.deltaTime);
                _currentAngle = Mathf.LerpAngle(_currentAngle, 0f, 5f * Time.deltaTime);
            }
        }
    }
}