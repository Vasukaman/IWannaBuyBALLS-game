// Filename: AutoActivatorModel.cs
using Core.Utilities;
using Services.Registry;
using System;
using System.Linq;
using UnityEngine;

namespace Gameplay.Gadgets
{
    public class AutoActivatorModel
    {
        public event Action<IActivatable> OnTargetActivated;
        public IActivatable CurrentTarget { get; private set; }

        private readonly Transform _ownerTransform;
        private readonly IActivatableRegistry _registry;
        private float _activationTimer;
        private float _scanTimer;

        private readonly float _activationInterval;
        private readonly float _scanInterval;

        public AutoActivatorModel(Transform ownerTransform, IActivatableRegistry registry, float activationInterval, float scanInterval)
        {
            _ownerTransform = ownerTransform;
            _registry = registry;
            _activationInterval = activationInterval;
            _scanInterval = scanInterval;
        }

        public void Tick(float deltaTime)
        {
            _scanTimer -= deltaTime;
            if (_scanTimer <= 0)
            {
                FindNearestTarget();
                _scanTimer = _scanInterval;
            }

            _activationTimer -= deltaTime;
            if (_activationTimer <= 0 && CurrentTarget != null)
            {
                OnTargetActivated?.Invoke(CurrentTarget);
                _activationTimer = _activationInterval;
            }
        }

        private void FindNearestTarget()
        {
            CurrentTarget = TargetingUtility.FindNearestTarget(_ownerTransform, _registry.AllActivatables);
        }
    }
}