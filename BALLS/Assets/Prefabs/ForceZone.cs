using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ForceZone : MonoBehaviour
{
    [Header("Force Settings")]
    [Tooltip("The layers that this force zone will affect.")]
    [SerializeField] private LayerMask targetLayers;

    [Tooltip("The direction the force will be applied in.")]
    [SerializeField] private Vector2 forceDirection = Vector2.up;

    [Tooltip("The strength of the force to apply.")]
    [SerializeField] private float forceMagnitude = 10f;

    [Tooltip("The type of force to apply. Force uses mass, Acceleration ignores mass.")]
    [SerializeField] private ForceMode2D forceMode = ForceMode2D.Force;

    private Collider2D _triggerCollider;

    private void Awake()
    {
        // Ensure the collider attached to this object is a trigger.
        _triggerCollider = GetComponent<Collider2D>();
        _triggerCollider.isTrigger = true;
    }

    /// <summary>
    /// This method is called continuously for every collider that stays within this trigger.
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        // First, check if the other object's layer is in our target layers mask.
        // This is an efficient bitwise operation.
        if ((targetLayers.value & (1 << other.gameObject.layer)) > 0)
        {
            // If the layer matches, then try to get its Rigidbody2D.
            if (other.TryGetComponent<Rigidbody2D>(out var rb))
            {
                // Apply the force to the rigidbody.
                rb.AddForce(forceDirection.normalized * forceMagnitude, forceMode);
            }
        }
    }
}