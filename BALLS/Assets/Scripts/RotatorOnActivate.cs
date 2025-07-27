using UnityEngine;
using System.Collections; // Required for Coroutines

public class RotatorOnActivate : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Assign the ManualActivator script that will trigger rotations.")]
    public ManualActivator manualActivator;

    [Tooltip("The GameObject to rotate. If null, this GameObject will rotate.")]
    public GameObject objectToRotate;

    [Header("Rotation Settings")]
    [Tooltip("Degrees to rotate each time the activator is triggered.")]
    [Range(1f, 360f)] // Ensure N is between 1 and 360
    public float rotationDegreesPerActivate = 90f;

    [Tooltip("Time in seconds for the rotation to complete smoothly.")]
    public float rotationDuration = 0.5f;

    [Tooltip("Type of easing for the rotation animation.")]
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Quaternion targetRotation; // The rotation we are smoothly moving towards
    private bool isRotating = false; // Flag to prevent multiple rotations at once

    void Awake()
    {
        // If objectToRotate is not assigned, default to this GameObject
        if (objectToRotate == null)
        {
            objectToRotate = this.gameObject;
        }

        // Initialize target rotation to current rotation to prevent immediate snap
        targetRotation = objectToRotate.transform.rotation;
    }

    void OnEnable()
    {
        // Subscribe to the OnActivate event
        if (manualActivator != null)
        {
            manualActivator.OnActivate += HandleActivation;
        }
        else
        {
            Debug.LogWarning("RotatorOnActivate: ManualActivator not assigned. Rotations will not be triggered.", this);
        }
    }

    void OnDisable()
    {
        // Unsubscribe to prevent memory leaks or errors if activator is destroyed
        if (manualActivator != null)
        {
            manualActivator.OnActivate -= HandleActivation;
        }
    }

    private void HandleActivation()
    {
        // If already rotating, ignore new activation to avoid jerky movements
        if (isRotating)
        {
            return;
        }

        // Calculate the new target rotation
        // We rotate around the Z-axis for 2D, or Y-axis for 3D vertical rotation
        // Adjust based on your object's orientation and desired axis of rotation
        targetRotation *= Quaternion.Euler(0, 0, rotationDegreesPerActivate); // For 2D (Z-axis)

        // Start the smooth rotation coroutine
        StartCoroutine(SmoothRotateCoroutine(objectToRotate.transform.rotation, targetRotation, rotationDuration));
    }

    private IEnumerator SmoothRotateCoroutine(Quaternion startRot, Quaternion endRot, float duration)
    {
        isRotating = true;
        float timer = 0f;

        while (timer < duration)
        {
            float progress = timer / duration;
            // Use the animation curve to modify the progress for easing effects
            float easedProgress = rotationCurve.Evaluate(progress);

            objectToRotate.transform.rotation = Quaternion.Lerp(startRot, endRot, easedProgress);

            timer += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure it snaps to the final rotation to avoid floating-point inaccuracies
        objectToRotate.transform.rotation = endRot;
        isRotating = false;
    }

    // Optional: Draw gizmos to see which object is set to rotate
    void OnDrawGizmosSelected()
    {
        if (objectToRotate != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(objectToRotate.transform.position, objectToRotate.transform.localScale * 1.1f);
            Gizmos.DrawLine(objectToRotate.transform.position, objectToRotate.transform.position + objectToRotate.transform.right * 0.5f);
            Gizmos.DrawLine(objectToRotate.transform.position, objectToRotate.transform.position + objectToRotate.transform.up * 0.5f);
        }
    }
}