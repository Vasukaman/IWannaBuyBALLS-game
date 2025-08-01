// Filename: ManualActivatorController.cs
using Reflex.Attributes;
using Services.Registry;
using System;
using UnityEngine;

namespace Gameplay.Gadgets
{
    [RequireComponent(typeof(Collider2D))]
    public class ManualActivatorController : MonoBehaviour, ICanConnect
    {
        [Header("Configuration")]
        [Tooltip("How often (in seconds) to rescan for the nearest target.")]
        [SerializeField] private float _targetScanInterval = 0.1f; // A frequent scan for visual responsiveness

        [Inject] private IButton _button;
        [Inject] private IActivatableRegistry _registry;

        private ManualActivatorModel _model;

        // --- ICanConnect Implementation ---
        public event Action OnActivate;
        public Transform GetStartTransform => this.transform;
        public Transform GetTargetTransform => _model?.CurrentTarget?.ActivationTransform;

        // --- Unity Methods ---

        private void Start()
        {
            if (_model == null)
            {
                // Pass the new interval to the model's constructor
                _model = new ManualActivatorModel(transform, _registry, _targetScanInterval);

                _model.OnTargetActivated += HandleTargetActivated;
            }
        }

        private void OnEnable()
        {
            if (_button != null) _button.OnClicked += OnButtonPressed; 
            if (_model != null) _model.OnTargetActivated += HandleTargetActivated;
        }

        private void OnDisable()
        {
            if (_button != null) _button.OnClicked -= OnButtonPressed;
            if (_model != null) _model.OnTargetActivated -= HandleTargetActivated;
        }

        private void Update()
        {
            // The Controller's only job in Update is to "tick" its brain.
            _model?.Tick(Time.deltaTime);
        }

        // --- Event Handlers ---

        private void OnButtonPressed()
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