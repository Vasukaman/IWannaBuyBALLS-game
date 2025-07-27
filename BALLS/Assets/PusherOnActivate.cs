using UnityEngine;

public class PusherOnActivate : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Assign the ManualActivator script that will trigger the push.")]
    public ManualActivator manualActivator;

    [Tooltip("The Rigidbody2D to push. If null, it will search on this GameObject.")]
    public Rigidbody2D bodyToPush;

    [Header("Push Settings")]
    [Tooltip("The maximum force applied as a single impulse.")]
    [Range(0.1f, 50f)]
    public float maxImpulse = 5f;

    void Awake()
    {
        // If bodyToPush is not assigned, try to find it on this GameObject.
        if (bodyToPush == null)
        {
            bodyToPush = GetComponent<Rigidbody2D>();
        }

        // Log a warning if no Rigidbody2D is found.
        if (bodyToPush == null)
        {
            Debug.LogWarning("PusherOnActivate: Rigidbody2D not found or assigned. The push will not work.", this);
        }
    }

    void OnEnable()
    {
        // Subscribe to the OnActivate event.
        if (manualActivator != null)
        {
            manualActivator.OnActivate += HandleActivation;
        }
        else
        {
            Debug.LogWarning("PusherOnActivate: ManualActivator not assigned. The push will not be triggered.", this);
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the event to prevent memory leaks.
        if (manualActivator != null)
        {
            manualActivator.OnActivate -= HandleActivation;
        }
    }

    /// <summary>
    /// Called when the ManualActivator's OnActivate event is invoked.
    /// </summary>
    private void HandleActivation()
    {
        // Ensure we have a Rigidbody2D to push before proceeding.
        if (bodyToPush == null)
        {
            return;
        }

        // 1. Get a random direction vector.
        // Random.insideUnitCircle returns a random point within a circle of radius 1.
        // .normalized ensures the vector has a magnitude of 1, so we get a pure direction.
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // 2. Calculate the final impulse vector.
        Vector2 impulse = randomDirection * maxImpulse;

        // 3. Apply the force to the Rigidbody2D.
        // ForceMode2D.Impulse applies the force instantly, which is ideal for a "push" or "kick".
        bodyToPush.AddForce(impulse, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Draws a gizmo in the editor to visualize which Rigidbody2D will be pushed.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (bodyToPush != null)
        {
            Gizmos.color = Color.red;
            // Draw a circle around the object to indicate it's the target.
            Gizmos.DrawWireSphere(bodyToPush.transform.position, 0.5f);
            // Draw an arrow from this script's object to the target Rigidbody2D.
            Gizmos.DrawLine(transform.position, bodyToPush.transform.position);
        }
    }
}