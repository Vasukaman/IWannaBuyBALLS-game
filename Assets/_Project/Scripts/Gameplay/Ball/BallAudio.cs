// Filename: BallAudioController.cs
// Location: _Project/Scripts/Gameplay/Ball/Behaviours/
using Core.Events;
using UnityEngine;

namespace Gameplay.BallSystem
{
    [RequireComponent(typeof(BallCollisionHandler))]
    public class BallAudioController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private SoundEffectProfile _collisionSound;
        [SerializeField] private float _soundCooldown = 0.2f;
        [SerializeField] private float _minVelocityForSound = 1.0f;

        [Header("Event Channel")]
        [Tooltip("The global event to raise when a valid collision occurs.")]
        [SerializeField] private SoundEffectGameEvent _onCollisionSoundEvent;

        private BallCollisionHandler _collisionHandler;
        private float _lastSoundTime;

        private void Awake()
        {
            _collisionHandler = GetComponent<BallCollisionHandler>();
        }

        private void OnEnable()
        {
            _collisionHandler.OnBallCollided += HandleBallCollision;
        }

        private void OnDisable()
        {
            _collisionHandler.OnBallCollided -= HandleBallCollision;
        }

        private void HandleBallCollision(Collision2D collision)
        {
            // Check if the cooldown has passed and if the impact was hard enough.
            if (Time.time < _lastSoundTime + _soundCooldown) return;
            if (collision.relativeVelocity.magnitude < _minVelocityForSound) return;

            // If all checks pass, raise the global event.
            _onCollisionSoundEvent.Raise(_collisionSound);
            
            _lastSoundTime = Time.time;
        }
    }
}