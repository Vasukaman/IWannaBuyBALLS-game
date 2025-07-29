using Game.Economy;
using System.Collections;
using System.Linq;
using UnityEngine;
using System; // Required for Action



public class AutoActivator : MonoBehaviour, ICanConnect
{
    [Tooltip("How often (in seconds) to automatically activate the nearest IActivatable.")]
    [SerializeField] private float interval = 1f;

    private IActivatable _currentTarget;

    public event Action OnActivate;
    public Transform GetStartTransform => this.transform;
    public Transform GetTargetTransform => _currentTarget.ActivationTransform;

    private void Update()
    {
        // find all IActivatables in the scene
        var all = GameObject
            .FindObjectsOfType<MonoBehaviour>()
            .OfType<IActivatable>()
            .ToList();

        if (all.Count == 0)
        {
            Debug.LogWarning("AutoActivator: No IActivatable found in scene.");
            return;
        }

        // pick the nearest one
        _currentTarget = all
            .OrderBy(a => Vector3.Distance(
                a.ActivationTransform.position,
                transform.position))
            .First();
    }

    private void Start()
    {
    
            StartCoroutine(AutoRoutine());
    }

    private IEnumerator AutoRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            if (_currentTarget != null)
            {
                _currentTarget.Activate();
                OnActivate?.Invoke();
            }

        }
    }

    /// <summary>
    /// Expose the position of the current target for your rope-dots to consume.
    /// </summary>
    public Transform TargetTransform => _currentTarget?.ActivationTransform;
}
