using UnityEngine;

[RequireComponent(typeof(Renderer), typeof(Collider2D))]
public class LiquidGate : MonoBehaviour
{
    private const int MAX_BALLS = 32;

    [Header("Rendering")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Bulge")]
    [SerializeField] private float maxBulge = 1f;
    [SerializeField] private float maxBulgePos = 0.8f;
    [SerializeField] private float minBulgePos = 0.8f;

    [Header("Wobble")]
    [SerializeField] private float spring = 90f;
    [SerializeField] private float damper = 10f;
    [SerializeField] private float lerpSpeed = 4f;

    [Header("Tilt & Noise")]
    [SerializeField] private float velocitySensitivity = 0.5f;
    [SerializeField] private float maxVelocityBulge = 0.8f;
    [SerializeField] private float noiseAmplitude = 0.05f;
    [SerializeField] private float maxTiltAngle = 35f;
    [SerializeField] private float tiltLerpSpeed = 5f;

    [Header("X-Ray Effect")]
    [SerializeField] private bool xrayForTopZone = true;
    [SerializeField] private Color xrayColorTop = new Color(0, 0.1f, 0.2f, 1);
    [SerializeField] private Color highlightColorTop = new Color(0.5f, 1, 1, 1);
    [Space]
    [SerializeField] private bool xrayForBottomZone = true;
    [SerializeField] private Color xrayColorBottom = new Color(0.2f, 0, 0.1f, 1);
    [SerializeField] private Color highlightColorBottom = new Color(1, 0.5f, 0.8f, 1);

    private MaterialPropertyBlock mpb;
    private Collider2D gateCollider;

    // Wobble state
    private float currentSize, currentPos, velocity;
    private Vector3 lastPos;

    // Tilt state
    private float targetAngle, currentAngle;

    // Ball data arrays
    private readonly Vector4[] ballDataArray = new Vector4[MAX_BALLS];
    private readonly Vector4[] ballColorArray = new Vector4[MAX_BALLS];

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        if (gateCollider == null) gateCollider = GetComponent<Collider2D>();
        gateCollider.isTrigger = true;
        lastPos = transform.position;
        currentPos = 0.5f;
    }

    void Update()
    {
        // Calculate velocity FIRST
        Vector3 vel = (transform.position - lastPos) / Time.deltaTime;
        lastPos = transform.position;
        
        HandleTilt(vel);
        HandleBallsAndBulge(vel);
        ApplyPropertiesToShader();
        
        // NEW: Force reset to zero when inactive
        ResetToZeroWhenIdle(vel);
    }

    // NEW METHOD: Reset to zero when completely idle
    private void ResetToZeroWhenIdle(Vector3 worldVelocity)
    {
        const float epsilon = 0.001f;
        bool isMoving = worldVelocity.magnitude > epsilon;
        bool hasBalls = mpb.GetInt("_BallCount") > 0;
        
        if (!isMoving && !hasBalls)
        {
            // Smoothly reset size and angle
            currentSize = Mathf.Lerp(currentSize, 0f, 5f * Time.deltaTime);
            velocity = Mathf.Lerp(velocity, 0f, 5f * Time.deltaTime);
            currentAngle = Mathf.LerpAngle(currentAngle, 0f, 5f * Time.deltaTime);
            
            // Snap to zero when close enough
            if (Mathf.Abs(currentSize) < epsilon && Mathf.Abs(velocity) < epsilon)
            {
                currentSize = 0f;
                velocity = 0f;
            }
            
            if (Mathf.Abs(currentAngle) < epsilon)
            {
                currentAngle = 0f;
            }
        }
    }

    private void HandleTilt(Vector3 worldVelocity)
    {
        // Only calculate tilt when moving
        if (worldVelocity.magnitude > 0.01f)
        {
            Vector3 localVel = transform.InverseTransformDirection(worldVelocity);
            float rawAngle = Mathf.Atan2(localVel.y, -localVel.x) * Mathf.Rad2Deg;
            rawAngle = Mathf.Clamp(rawAngle, -maxTiltAngle, maxTiltAngle);
            rawAngle *= Mathf.Sign(currentSize);
            targetAngle = rawAngle;
        }
        else
        {
            // NEW: Reset target angle when not moving
            targetAngle = 0f;
        }

        // Always apply smoothing
        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, tiltLerpSpeed * Time.deltaTime);
    }
    private void HandleBallsAndBulge(Vector3 worldVelocity)
    {
        // Find all balls within this gate's trigger area
        Collider2D[] colliders = new Collider2D[MAX_BALLS];
        int ballCount = gateCollider.OverlapCollider(new ContactFilter2D().NoFilter(), colliders);

        float targetSize = 0f;
        float targetPos = 0.5f;
        float closestDistY = float.MaxValue;

        int validBallIdx = 0;
        for (int i = 0; i < ballCount; i++)
        {
            if (validBallIdx >= MAX_BALLS) break;

            Ball b = colliders[i].GetComponent<Ball>();
            if (!b) continue;

            // Convert ball world position to gate's local UV space (0-1)
            Vector3 localPos = transform.InverseTransformPoint(b.transform.position);
            Vector2 uvPos = new Vector2(localPos.x + 0.5f, localPos.y + 0.5f);

            // Convert ball world radius to local UV radius
            float uvRadius = b.GetShaderTrueSize() / transform.lossyScale.x;

            ballDataArray[validBallIdx] = new Vector4(uvPos.x, uvPos.y, uvRadius, 0);
            ballColorArray[validBallIdx] = b.color;
            validBallIdx++;

            // Find closest ball for bulge effect
            float distY = Mathf.Abs(localPos.y);
            if (distY < closestDistY)
            {
                closestDistY = distY;
                if (distY <= uvRadius * 1.5f)
                {
                    targetSize = Mathf.Min(uvRadius * 2f, maxBulge);
                    targetPos = uvPos.x;
                }
            }
        }
        mpb.SetInt("_BallCount", validBallIdx);

        // Add velocity-based bulge
        // Add velocity-based bulge ONLY when moving
        Vector3 localVel = transform.InverseTransformDirection(worldVelocity);
        float tiltBulge = 0f;

        if (worldVelocity.magnitude > 0.01f)
        {
            tiltBulge = Mathf.Clamp(localVel.y * velocitySensitivity,
                                    -maxVelocityBulge, maxVelocityBulge);
        }


        // Apply physics to combined bulge
        float combinedTarget = targetSize + tiltBulge;
        float force = (combinedTarget - currentSize) * spring;
        velocity = (velocity + force * Time.deltaTime) * Mathf.Exp(-damper * Time.deltaTime);
        currentSize += velocity * Time.deltaTime;
        currentSize = Mathf.Clamp(currentSize, -maxBulge, maxBulge);
        targetPos = Mathf.Clamp(targetPos, minBulgePos, maxBulgePos);
        currentPos = Mathf.MoveTowards(currentPos, targetPos, lerpSpeed * Time.deltaTime);
    }

    private void ApplyPropertiesToShader()
    {
        // Push Bulge/Wobble/Tilt properties
        mpb.SetFloat("_BulgeSize", -currentSize);
        mpb.SetFloat("_BulgePos", currentPos);
        mpb.SetFloat("_BulgeAngle", currentAngle);

        // Adjust noise based on current bulge state
        float amp = Mathf.Lerp(noiseAmplitude, 0f, Mathf.Abs(currentSize) / maxBulge);
        mpb.SetFloat("_NoiseAmp", amp);

        // Push X-Ray properties
        mpb.SetFloat("_XRayTopEnable", xrayForTopZone ? 1.0f : 0.0f);
        mpb.SetColor("_XRayColorTop", xrayColorTop);
        mpb.SetColor("_HighlightColorTop", highlightColorTop);

        mpb.SetFloat("_XRayBottomEnable", xrayForBottomZone ? 1.0f : 0.0f);
        mpb.SetColor("_XRayColorBottom", xrayColorBottom);
        mpb.SetColor("_HighlightColorBottom", highlightColorBottom);

        // Push ball data arrays
        mpb.SetVectorArray("_BallData", ballDataArray);
        mpb.SetVectorArray("_BallColors", ballColorArray);

        targetRenderer.SetPropertyBlock(mpb);
    }
}