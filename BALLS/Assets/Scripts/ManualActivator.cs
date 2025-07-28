using Game.Economy;
using Reflex.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System; // Required for Action

public class ManualActivator : MonoBehaviour, IActivator
{
    private IActivatable _currentTarget;
    [Inject] private IButton _button;

    // NEW: Public event for other scripts to subscribe to
    public event Action OnActivate;

    public Transform GetStartTransform => this.transform;
    public Transform GetTargetTransform => _currentTarget.ActivationTransform;


    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.OnClicked -= OnButtonClicked;
        }
    }


    private void Start()
    {
        _button.OnClicked += OnButtonClicked;
    }

    private void Update()
    {
        // Update target every frame
        UpdateTarget();
    }

    private void UpdateTarget()
    {
        // Find all IActivatables in the scene
        var allActivatables = GameObject
            .FindObjectsOfType<MonoBehaviour>()
            .OfType<IActivatable>()
            .ToList();

        if (allActivatables.Count == 0)
        {
            _currentTarget = null;
            return;
        }

        // Find the nearest activatable
        _currentTarget = allActivatables
            .OrderBy(a => Vector3.Distance(
                a.ActivationTransform.position,
                transform.position))
            .First();
    }

    private void OnButtonClicked()
    {
        if (_currentTarget != null)
        {
            _currentTarget.Activate();
            // NEW: Invoke the OnActivate event after successful activation
            OnActivate?.Invoke();
        }
    }

    /// <summary>
    /// Expose the position of the current target for rope visualization
    /// </summary>
    public Transform TargetTransform => _currentTarget?.ActivationTransform;
}