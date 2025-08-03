// Filename: ManualActivatorModel.cs
using Core.Utilities;
using Gameplay.Interfaces;
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

        // The constructor now takes the base profile.
        public ManualActivatorModel(Transform ownerTransform, IActivatableRegistry registry, ManualActivatorProfile profile)
        {
            _ownerTransform = ownerTransform;
            _registry = registry;
            _scanInterval = profile.TargetScanInterval;
        }

        public void Tick(float deltaTime)
        {
            _scanTimer -= deltaTime;
            if (_scanTimer <= 0f)
            {
                UpdateTarget();
                _scanTimer = _scanInterval;
            }
        }

        private void UpdateTarget()
        {
            CurrentTarget = TargetingUtility.FindNearestTarget(_ownerTransform, _registry.AllActivatables);
        }

        public void ActivateCurrentTarget()
        {
            if (CurrentTarget != null)
            {
                OnTargetActivated?.Invoke(CurrentTarget);
            }
        }
    }
}