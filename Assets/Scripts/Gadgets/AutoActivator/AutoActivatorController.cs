// Filename: AutoActivatorController.cs
using Reflex.Attributes;
using Services.Registry;
using System;
using UnityEngine;
using Services.Money;
using Gameplay.Interfaces;
namespace Gameplay.Gadgets
{
    public class AutoActivatorController : MonoBehaviour, ICanConnect, IActivationSource
    {
        [Header("Configuration")]
        [SerializeField] private float _activationInterval = 1f;
        [SerializeField] private float _targetScanInterval = 2.0f;

        // This is guaranteed to be injected before Start().
        [Inject] private IActivatableRegistry _registry;
        [Inject] private IMoneyService _moneyService;

        private AutoActivatorModel _model;

        // --- ICanConnect Implementation ---
        public event Action OnActivate;
        public Transform GetStartTransform => this.transform;
        public Transform GetTargetTransform => _model?.CurrentTarget?.ActivationTransform;

        // --- Unity Methods ---

        // Start is called only once in a component's lifetime, after all injections are complete.
        // This is the perfect place for one-time setup.
        private void Start()
        {
            // This check ensures the model is only ever created once.
            if (_model == null)
            {
                _model = new AutoActivatorModel(transform, _registry, _activationInterval, _targetScanInterval);
                _model.OnTargetActivated += HandleTargetActivated;
            }
        }

        // OnEnable can be called multiple times (if the object is pooled).
        // It's used for actions that need to happen every time the object becomes active.
        private void OnEnable()
        {
           
            // We must check if the model exists yet, because OnEnable can be called before Start.
            if (_model != null)
            {
                _model.OnTargetActivated += HandleTargetActivated;
            }
        }

        private void OnDisable()
        {
            // Always unsubscribe here to prevent "zombie listeners".
            if (_model != null)
            {
                _model.OnTargetActivated -= HandleTargetActivated;
            }
        }

        private void Update()
        {
            // The model will only be ticked after it has been created in Start().
            _model?.Tick(Time.deltaTime);
        }

        private void HandleTargetActivated(IActivatable target)
        {
            target.Activate();
            OnActivate?.Invoke();
        }
    }
}