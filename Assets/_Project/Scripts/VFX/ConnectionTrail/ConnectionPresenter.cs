// Filename: ConnectionPresenter.cs
using Gameplay.Interfaces;
using UnityEngine;
using VFX.Interfaces; // Use the new interface namespace

namespace Gameplay.Gadgets
{
    /// <summary>
    /// The "Presenter" for a visual connection. It coordinates between a logical connection source
    /// (IConnectionSource) and any visual effect that implements ITargetableVisual.
    /// </summary>
    // The RequireComponent now looks for the INTERFACE, not the class.
    [RequireComponent(typeof(IConnectionSource), typeof(ITargetableVisual))]
    public class ConnectionPresenter : MonoBehaviour
    {
        // --- Component References ---
        private IConnectionSource _source;
        private ITargetableVisual _visual; // The type is now the interface

        private void Awake()
        {
            _source = GetComponent<IConnectionSource>();
            // It gets any component that fulfills the contract.
            _visual = GetComponent<ITargetableVisual>();
        }

        private void Update()
        {
            // The logic remains identical, but it's now completely decoupled.
            if (_source != null && _visual != null)
            {
                _visual.Target = _source.TargetTransform;
            }
        }
    }
}