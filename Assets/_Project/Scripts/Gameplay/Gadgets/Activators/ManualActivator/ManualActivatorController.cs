// Filename: ManualActivatorController.cs
using Core.Events;
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
        [Header("Configuration")]
        [Tooltip("The ScriptableObject asset that defines the timing for this activator.")]
        [SerializeField] private ManualActivatorProfile _profile; // It now asks for its specific profile

        [Header("Event Channel")]
        [SerializeField] private GameEvent _manualActivationEvent;

        [Inject] private IActivatableRegistry _registry;

        private ManualActivatorModel _model;

        public event Action OnActivate;
        public Transform StartTransform => this.transform;
        public Transform TargetTransform => _model?.CurrentTarget?.ActivationTransform;


        private void Start()
        {
            if (_profile == null) { Debug.LogError("AutoActivatorProfile is not assigned to AutoActivator!", this); return; }
            if (_model == null)
            {
                _model = new ManualActivatorModel(transform, _registry, _profile);
                _model.OnTargetActivated += HandleTargetActivated;
            }
        }


        private void OnEnable()
        {
            if (_manualActivationEvent != null) _manualActivationEvent.RegisterListener(OnActivationSignalReceived);
            if (_model != null) _model.OnTargetActivated += HandleTargetActivated;
        }

        private void OnDisable()
        {
            if (_manualActivationEvent != null) _manualActivationEvent.UnregisterListener(OnActivationSignalReceived);
            if (_model != null) _model.OnTargetActivated -= HandleTargetActivated;
        }

        private void Update() => _model?.Tick(Time.deltaTime);
        private void OnActivationSignalReceived() => _model?.ActivateCurrentTarget();
        private void HandleTargetActivated(IActivatable target)
        {
            target.Activate();
            OnActivate?.Invoke();
        }
    }
}