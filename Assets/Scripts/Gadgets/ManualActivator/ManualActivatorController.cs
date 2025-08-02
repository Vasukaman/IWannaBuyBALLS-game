// Filename: ManualActivatorController.cs
using Core.Events; // Use the new event namespace
using Gameplay.Interfaces;
using Reflex.Attributes;
using Services.Registry;
using System;
using UnityEngine;

namespace Gameplay.Gadgets
{
    [RequireComponent(typeof(Collider2D))]
    public class ManualActivatorController : MonoBehaviour, IConnectionSource, IActivationSource
    {
        [Header("Event Channel")]
        [Tooltip("The GameEvent asset this component listens to for activation signals.")]
        [SerializeField] private GameEvent _manualActivationEvent;

        [Header("Configuration")]
        [SerializeField] private float _targetScanInterval = 0.1f;

        [Inject] private IActivatableRegistry _registry;

        private ManualActivatorModel _model;

        // --- IConnectionSource Implementation ---
        public event Action OnActivate;
        public Transform StartTransform => this.transform;
        public Transform TargetTransform => _model?.CurrentTarget?.ActivationTransform;

        // --- Unity Methods ---

        private void Start()
        {
            if (_model == null)
            {
                _model = new ManualActivatorModel(transform, _registry, _targetScanInterval);
                _model.OnTargetActivated += HandleTargetActivated;
            }
        }

        private void OnEnable()
        {
            if (_manualActivationEvent != null)
            {
                _manualActivationEvent.RegisterListener(OnActivationSignalReceived);
            }
        }

        private void OnDisable()
        {
            if (_manualActivationEvent != null)
            {
                _manualActivationEvent.UnregisterListener(OnActivationSignalReceived);
            }
        }

        private void Update()
        {
            _model?.Tick(Time.deltaTime);
        }

        // --- Event Handlers ---

        private void OnActivationSignalReceived()
        {
            _model?.ActivateCurrentTarget();
        }

        private void HandleTargetActivated(IActivatable target)
        {
            target.Activate();
            OnActivate?.Invoke();
        }
    }
}