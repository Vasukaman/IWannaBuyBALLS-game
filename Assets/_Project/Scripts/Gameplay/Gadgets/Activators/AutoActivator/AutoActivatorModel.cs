// Filename: AutoActivatorModel.cs
using Core.Utilities;
using Gameplay.Interfaces;
using Services.Registry;
using System;
using UnityEngine;

namespace Gameplay.Gadgets
{
    public class AutoActivatorModel
    {
        public event Action<IActivatable> OnTargetActivated;
        public IActivatable CurrentTarget { get; private set; }

        private readonly Transform _ownerTransform;
        private readonly IActivatableRegistry _registry;
        private readonly float _activationInterval;
        private readonly float _scanInterval;
        private float _activationTimer;
        private float _scanTimer;

        // The constructor now takes the specialized auto-activator profile.
        public AutoActivatorModel(Transform ownerTransform, IActivatableRegistry registry, AutoActivatorProfile profile)
        {
            _ownerTransform = ownerTransform;
            _registry = registry;
            // It can access properties from BOTH the base and the child class.
            _activationInterval = profile.ActivationInterval;
            _scanInterval = profile.TargetScanInterval;
        }

        public void Tick(float deltaTime)
        {
            _scanTimer -= deltaTime;
            if (_scanTimer <= 0)
            {
                UpdateTarget();
                _scanTimer = _scanInterval;
            }

            _activationTimer -= deltaTime;
            if (_activationTimer <= 0 && CurrentTarget != null)
            {
                OnTargetActivated?.Invoke(CurrentTarget);
                _activationTimer = _activationInterval;
            }
        }

        private void UpdateTarget()
        {
            CurrentTarget = TargetingUtility.FindNearestTarget(_ownerTransform, _registry.AllActivatables);
        }
    }
}