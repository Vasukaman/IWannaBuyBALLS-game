using Game.Economy;
using Reflex.Attributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ManualActivator : MonoBehaviour, ICanConnect
{
    private IActivatable _currentTarget;
    [Inject] private IButton _button;
    public Transform GetStartTransform => this.transform;
    public Transform GetEndTransform => _currentTarget.ActivationTransform;


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
        }
    }

    /// <summary>
    /// Expose the position of the current target for rope visualization
    /// </summary>
    public Transform TargetTransform => _currentTarget?.ActivationTransform;
}