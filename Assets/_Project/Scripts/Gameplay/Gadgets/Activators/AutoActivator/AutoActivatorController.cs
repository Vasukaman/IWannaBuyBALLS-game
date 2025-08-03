// Filename: AutoActivatorController.cs
using Gameplay.Interfaces;
using Reflex.Attributes;
using Services.Registry;
using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.Gadgets
{
    public class AutoActivatorController : MonoBehaviour, IConnectionSource, IActivationSource
    {
        [Header("Configuration")]
        [Tooltip("The ScriptableObject asset that defines the timing for this activator.")]
        [SerializeField] private AutoActivatorProfile _profile; // It asks for the specialized profile.

        [Inject] private IActivatableRegistry _registry;

        private AutoActivatorModel _model;

        // --- ICanConnect Implementation ---
        public event Action OnActivate;
        public Transform StartTransform => this.transform;
        public Transform TargetTransform => _model?.CurrentTarget?.ActivationTransform;

        // --- Unity Methods ---

        // Start is called only once in a component's lifetime, after all injections are complete.
        // This is the perfect place for one-time setup.
         private void Start()
        {
          
            if (_profile == null) 
            {
                Debug.LogError("AutoActivatorProfile is not assigned to AutoActivator!", this);
                enabled = false;
                return;
            }
            if (_model == null)
            {
                _model = new AutoActivatorModel(transform, _registry, _profile);
                //Wierd fix to solve injection race condition. It inject AFTER awake and enabled.
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