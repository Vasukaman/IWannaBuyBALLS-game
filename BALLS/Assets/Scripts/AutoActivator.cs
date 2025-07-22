using System.Collections;
using System.Linq;
using UnityEngine;

public class AutoActivator : MonoBehaviour
{
    [Tooltip("How often (in seconds) to automatically activate the nearest IActivatable.")]
    [SerializeField] private float interval = 1f;

    private IActivatable target;

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
        target = all
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
            if (target != null)
                target.Activate();
        }
    }

    /// <summary>
    /// Expose the position of the current target for your rope-dots to consume.
    /// </summary>
    public Transform TargetTransform => target?.ActivationTransform;
}
