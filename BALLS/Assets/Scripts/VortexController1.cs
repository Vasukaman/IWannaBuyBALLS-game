using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Renderer), typeof(Collider2D))]
public class VortexController : MonoBehaviour
{
    private const int MAX_BALLS = 32;

    [Header("Rendering")]
    [Tooltip("The Renderer component that has the VortexBallDrawer material.")]
    [SerializeField] private Renderer vortexRenderer;

    private MaterialPropertyBlock mpb;
    private Collider2D vortexCollider;

    // We use a Dictionary to track balls currently inside the vortex trigger
    private readonly Dictionary<Ball, bool> ballsInVortex = new Dictionary<Ball, bool>();

    // Ball data arrays to be sent to the shader
    private readonly Vector4[] ballDataArray = new Vector4[MAX_BALLS];
    private readonly Vector4[] ballColorArray = new Vector4[MAX_BALLS];

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        if (vortexRenderer == null) vortexRenderer = GetComponent<Renderer>();
        if (vortexCollider == null) vortexCollider = GetComponent<Collider2D>();
        vortexCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null && !ballsInVortex.ContainsKey(ball))
        {
            ballsInVortex.Add(ball, true);
            ball.GetComponentInChildren<Renderer>().enabled = false; // Hide original ball
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Ball ball = other.GetComponent<Ball>();
        if (ball != null && ballsInVortex.ContainsKey(ball))
        {
            ballsInVortex.Remove(ball);
            ball.GetComponentInChildren<Renderer>().enabled = true; // Show original if it leaves
        }
    }

    void Update()
    {
        UpdateBallData();
        ApplyPropertiesToShader();
    }

    private void UpdateBallData()
    {
        int validBallIdx = 0;
        // It's safer to create a temporary list for iteration
        // in case a ball gets destroyed while in the vortex.
        var ballsToRemove = new List<Ball>();

        foreach (var ball in ballsInVortex.Keys)
        {
            if (validBallIdx >= MAX_BALLS) break;

            // If ball was destroyed, mark for removal
            if (ball == null)
            {
                ballsToRemove.Add(ball);
                continue;
            }

            // Convert ball world position to this object's local UV space (0-1)
            Vector3 localPos = transform.InverseTransformPoint(ball.transform.position);
            Vector2 uvPos = new Vector2(localPos.x + 0.5f, localPos.y + 0.5f);

            // Convert ball world radius to local UV radius
            // Assuming the Ball script has a way to get its radius
            float uvRadius = ball.transform.localScale.x * 0.5f / transform.lossyScale.x;

            ballDataArray[validBallIdx] = new Vector4(uvPos.x, uvPos.y, uvRadius, 0);
            ballColorArray[validBallIdx] = ball.GetComponentInChildren<Renderer>().material.color; // Or a specific property
            validBallIdx++;
        }

        // Clean up any destroyed balls
        foreach (var ball in ballsToRemove)
        {
            ballsInVortex.Remove(ball);
        }

        mpb.SetInt("_BallCount", validBallIdx);
    }

    private void ApplyPropertiesToShader()
    {
        // Get the specific material instance for the vortex effect
        vortexRenderer.GetPropertyBlock(mpb);

        // Push ball data arrays to the shader
        mpb.SetVectorArray("_BallData", ballDataArray);
        mpb.SetVectorArray("_BallColors", ballColorArray);

        vortexRenderer.SetPropertyBlock(mpb);
    }
}