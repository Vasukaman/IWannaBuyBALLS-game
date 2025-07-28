using Game.Economy;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public struct DotParams
{
    public float amplitude;      // Current wave height
    public float frequency;      // Current wave density
    public float speed;          // Current movement speed (units per second)
    public float spawnTime;      // Creation timestamp
    public float duration;       // How long it takes for the dot to reach its target

    // Store initial values for gradual change
    public float initialAmplitude;
    public float initialFrequency;
    public float initialSpeed;
    public float initialScale;   // NEW: Initial scale for gradual change
}
public class RopeDot
{
    public Transform transform;
    public Vector3 startPosition;
    public Vector3 targetPosition;
    public DotParams parameters;
    public bool isActive;

    public int dotID; // For debugging, optional

    // NEW: Pass a global wave offset value to this method
    public Vector3 GetCurrentPosition(float globalWaveOffset)
    {
        if (!isActive) return transform.position;

        float t = (Time.time - parameters.spawnTime) / parameters.duration;
        float progress = Mathf.Clamp01(t);

        Vector3 basePos = Vector3.Lerp(startPosition, targetPosition, progress);

        Vector3 pathDir = (targetPosition - startPosition).normalized;
        Vector3 perpendicular = new Vector3(-pathDir.y, pathDir.x, 0);

        // Calculate the wave offset, now including the global wave movement
        // The globalWaveOffset adds a constant phase shift to the wave for all dots
        float waveOffset = parameters.amplitude * Mathf.Sin((progress * parameters.frequency * 2 * Mathf.PI) + globalWaveOffset);

        return basePos + perpendicular * waveOffset;
    }

    // UpdatePosition will now need the global wave offset
    public void UpdatePosition(float globalWaveOffset)
    {
        if (isActive)
        {
            transform.position = GetCurrentPosition(globalWaveOffset);
        }
    }

    public bool HasReachedTarget()
    {
        float t = (Time.time - parameters.spawnTime) / parameters.duration;
        return t >= 1.0f;
    }
}
public class DottedRope : MonoBehaviour
{
    public IActivator connecter; // Assign this in the Inspector (the spawner/target)

    [Header("Dot Spawning Settings")]
    public GameObject dotPrefab;
    public int maxDots = 50; // Increased maxDots for potentially longer trails
    public float spawnInterval = 0.1f;

    [Header("Base Wave Parameters")]
    [Tooltip("Base amplitude for the wave.")]
    public float baseAmplitude = 0.2f;
    [Tooltip("Base frequency for the wave (cycles per path length).")]
    public float baseFrequency = 2f;
    [Tooltip("Base speed at which dots travel (units per second).")]
    public float baseSpeed = 1f;
    [Tooltip("Base speed at which the wave itself progresses globally.")]
    public float baseGlobalWaveSpeed = 5.0f; // Base speed for GLOBAL wave progression

    [Header("Randomness Control")]
    [Tooltip("Enable or disable randomness for dot parameters.")]
    public bool enableRandomness = true;

    [Tooltip("Max random deviation for amplitude (e.g., 0.05 means +/- 0.05).")]
    [Range(0f, 0.5f)] // Clamp range for better control in editor
    public float randomAmplitudeDeviation = 0.05f;
    [Tooltip("Max random deviation for frequency (e.g., 0.5 means +/- 0.5).")]
    [Range(0f, 5f)] // Clamp range
    public float randomFrequencyDeviation = 0.5f;
    [Tooltip("Max random deviation for speed (e.g., 0.2 means +/- 0.2).")]
    [Range(0f, 1f)] // Clamp range
    public float randomSpeedDeviation = 0.2f;
    [Tooltip("Max random deviation for dot initial scale.")]
    [Range(0f, 0.5f)]
    public float randomScaleDeviation = 0.05f;


    [Header("Gradual Parameter Change (per dot)")]
    [Tooltip("Enable gradual change for parameters as dots travel.")]
    public bool enableGradualChange = true;

    [Space(5)]
    [Tooltip("Multiplier for frequency at the start of the dot's journey.")]
    public float startFrequencyMultiplier = 1.0f;
    [Tooltip("Multiplier for frequency at the end of the dot's journey.")]
    public float endFrequencyMultiplier = 2.0f;

    [Space(5)]
    [Tooltip("Multiplier for amplitude at the start of the dot's journey.")]
    public float startAmplitudeMultiplier = 1.0f;
    [Tooltip("Multiplier for amplitude at the end of the dot's journey.")]
    public float endAmplitudeMultiplier = 0.5f;

    [Space(5)]
    [Tooltip("Multiplier for speed at the start of the dot's journey.")]
    public float startSpeedMultiplier = 1.0f;
    [Tooltip("Multiplier for speed at the end of the dot's journey.")]
    public float endSpeedMultiplier = 1.0f;

    [Space(5)]
    [Tooltip("Multiplier for dot scale at the start of the dot's journey.")]
    public float startScaleMultiplier = 1.0f;
    [Tooltip("Multiplier for dot scale at the end of the dot's journey.")]
    public float endScaleMultiplier = 0.5f;

    // NEW: Gradual global wave speed change
    [Header("Global Wave Speed Control")]
    [Tooltip("Enable gradual change for the global wave speed.")]
    public bool enableGradualGlobalWaveSpeed = false;
    [Tooltip("Multiplier for global wave speed at the start of the rope's lifetime.")]
    public float startGlobalWaveSpeedMultiplier = 1.0f;
    [Tooltip("Multiplier for global wave speed at the end of the rope's lifetime.")]
    public float endGlobalWaveSpeedMultiplier = 1.0f;
    // To track the total duration or "lifetime" of the rope for global gradual changes
    [Tooltip("The time in seconds over which the global wave speed gradually changes.")]
    public float globalWaveSpeedDuration = 10f;
    private float ropeStartTime; // To track when this DottedRope instance started

    private List<RopeDot> dots = new List<RopeDot>();
    private Transform currentTarget;
    private float lastSpawnTime;
    private float globalWaveOffset = 0f; // Global offset for the entire wave

    // Property to get the origin of the rope (this GameObject's position)
    public Vector3 RopeOrigin => transform.position;

    void Start()
    {
        // Initialize the rope's start time for global gradual changes
        ropeStartTime = Time.time;
        connecter = GetComponent<IActivator>();
    }

    void Update()
    {
        // Auto-set target for testing. In a real game, this would be set by other game logic.
        if (connecter != null)
            SetNewTarget(connecter.GetTargetTransform); // Ensure AutoActivator has a public TargetTransform
        else if (currentTarget == null)
        {
        //    Debug.LogWarning("DottedRope: No AutoActivator assigned, and no currentTarget set. Dots won't spawn.");
            return; // Don't try to update or spawn if no target
        }

        // --- FIX: Ensure currentGlobalWaveSpeed is updated based on base value and gradual change ---
        float currentCalculatedGlobalWaveSpeed = baseGlobalWaveSpeed; // Start with base value

        if (enableGradualGlobalWaveSpeed)
        {
            float timeSinceRopeStart = Time.time - ropeStartTime;
            float globalProgress = Mathf.Clamp01(timeSinceRopeStart / globalWaveSpeedDuration);
            float globalSpeedMultiplier = Mathf.Lerp(startGlobalWaveSpeedMultiplier, endGlobalWaveSpeedMultiplier, globalProgress);
            currentCalculatedGlobalWaveSpeed *= globalSpeedMultiplier; // Apply multiplier to the base speed
        }

        // Update the global wave offset using the potentially modified global wave speed
        globalWaveOffset += currentCalculatedGlobalWaveSpeed * Time.deltaTime;


        // Update all dots
        foreach (var dot in dots)
        {
            if (dot.isActive)
            {
                float progress = (Time.time - dot.parameters.spawnTime) / dot.parameters.duration;
                progress = Mathf.Clamp01(progress);

                // If gradual change is enabled, re-calculate parameters based on current progress
                if (enableGradualChange)
                {
                    // Apply gradual change to frequency
                    float currentFrequencyMultiplier = Mathf.Lerp(startFrequencyMultiplier, endFrequencyMultiplier, progress);
                    dot.parameters.frequency = dot.parameters.initialFrequency * currentFrequencyMultiplier;

                    // Apply gradual change to amplitude
                    float currentAmplitudeMultiplier = Mathf.Lerp(startAmplitudeMultiplier, endAmplitudeMultiplier, progress);
                    dot.parameters.amplitude = dot.parameters.initialAmplitude * currentAmplitudeMultiplier;

                    // Apply gradual change to speed (which affects duration for recalculation)
                    float currentSpeedMultiplier = Mathf.Lerp(startSpeedMultiplier, endSpeedMultiplier, progress);
                    dot.parameters.speed = dot.parameters.initialSpeed * currentSpeedMultiplier;

                    // Apply gradual change to dot scale
                    float currentScaleMultiplier = Mathf.Lerp(startScaleMultiplier, endScaleMultiplier, progress);
                    dot.transform.localScale = Vector3.one * (dot.parameters.initialScale * currentScaleMultiplier);

                    // Re-calculate duration based on the new speed
                    float distance = Vector3.Distance(dot.startPosition, dot.targetPosition);
                    dot.parameters.duration = dot.parameters.speed > 0.001f ? distance / dot.parameters.speed : 1000f;
                }

                // Pass the global wave offset to the dot's position calculation
                dot.UpdatePosition(globalWaveOffset);

                if (dot.HasReachedTarget())
                {
                    dot.isActive = false;
                    dot.transform.gameObject.SetActive(false); // Hide the GameObject when inactive
                }
            }
        }

        // Spawn new dots
        if (ShouldSpawnDot())
        {
            SpawnDot();
        }
    }

    public void SetNewTarget(Transform newTarget)
    {
        currentTarget = newTarget;
    }

    private bool ShouldSpawnDot()
    {
        bool canSpawnMore = dots.Count < maxDots || dots.Any(d => !d.isActive);
        return currentTarget != null &&
               Time.time >= lastSpawnTime + spawnInterval &&
               canSpawnMore;
    }

    private void SpawnDot()
    {
        RopeDot dot = dots.Find(d => !d.isActive);

        if (dot == null)
        {
            if (dots.Count < maxDots)
            {
                GameObject newDotGO = Instantiate(dotPrefab, RopeOrigin, Quaternion.identity);
                dot = new RopeDot
                {
                    transform = newDotGO.transform,
                    parameters = new DotParams()
                };
                dots.Add(dot);
            }
            else
            {
                Debug.LogWarning("DottedRope: Max dots reached and no inactive dots available for reuse.");
                return;
            }
        }
        else
        {
            dot.transform.gameObject.SetActive(true);
        }

        // Configure dot
        dot.startPosition = RopeOrigin;
        dot.targetPosition = currentTarget.position;

        // Apply base parameters first
        dot.parameters.amplitude = baseAmplitude;
        dot.parameters.frequency = baseFrequency;
        dot.parameters.speed = baseSpeed;

        // Apply randomness if enabled
        if (enableRandomness)
        {
            dot.parameters.amplitude += Random.Range(-randomAmplitudeDeviation, randomAmplitudeDeviation);
            dot.parameters.frequency += Random.Range(-randomFrequencyDeviation, randomFrequencyDeviation);
            dot.parameters.speed += Random.Range(-randomSpeedDeviation, randomSpeedDeviation);
        }

        // Store initial values for gradual change calculation
        dot.parameters.initialAmplitude = dot.parameters.amplitude;
        dot.parameters.initialFrequency = dot.parameters.frequency;
        dot.parameters.initialSpeed = dot.parameters.speed;

        // Calculate duration based on potentially randomized speed
        float distance = Vector3.Distance(dot.startPosition, dot.targetPosition);
        dot.parameters.duration = dot.parameters.speed > 0.001f ? distance / dot.parameters.speed : 1000f;

        dot.parameters.spawnTime = Time.time;
        dot.isActive = true;

        // Apply initial scale, with randomness if enabled, and store it
        float initialScale = 0.2f; // Base scale, adjust as needed
        if (enableRandomness)
        {
            initialScale += Random.Range(-randomScaleDeviation, randomScaleDeviation);
        }
        dot.parameters.initialScale = initialScale; // Store initial scale
        dot.transform.localScale = Vector3.one * initialScale;

        lastSpawnTime = Time.time;
    }
}