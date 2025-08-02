// Filename: ManualActivatorModel.cs
using Core.Utilities; // For the TargetingUtility
using Services.Registry;
using System;
using UnityEngine;

namespace Gameplay.Gadgets
{
    public class ManualActivatorModel
    {
        public event Action<IActivatable> OnTargetActivated;
        public IActivatable CurrentTarget { get; private set; }

        private readonly Transform _ownerTransform;
        private readonly IActivatableRegistry _registry;
        private readonly float _scanInterval;
        private float _scanTimer;

        public ManualActivatorModel(Transform ownerTransform, IActivatableRegistry registry, float scanInterval)
        {
            _ownerTransform = ownerTransform;
            _registry = registry;
            _scanInterval = scanInterval;
        }

        /// <summary>
        /// Called every frame by the Controller to update the model's internal state.
        /// </summary>
        public void Tick(float deltaTime)
        {
            _scanTimer -= deltaTime;
            if (_scanTimer <= 0f)
            {
                UpdateTarget();
                _scanTimer = _scanInterval;
            }
        }

        /// <summary>
        /// Scans the registry to find the nearest valid target.
        /// </summary>
        private void UpdateTarget()
        {
            CurrentTarget = TargetingUtility.FindNearestTarget(_ownerTransform, _registry.AllActivatables);
        }

        /// <summary>
        /// Triggers the activation event for the current target if one exists.
        /// </summary>
        public void ActivateCurrentTarget()
        {
            if (CurrentTarget != null)
            {
                OnTargetActivated?.Invoke(CurrentTarget);
            }
        }
    }
}