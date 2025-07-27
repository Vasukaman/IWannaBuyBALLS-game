// C# Controller Script: VortexController.cs
// Attach this script to the 2D Sprite that will have the vortex effect.

using UnityEngine;

[ExecuteInEditMode] // Allows the effect to be visible in the editor without running the game
public class VortexController : MonoBehaviour
{
    // Assign the Material that uses the Vortex shader in the Inspector
    public Material vortexMaterial;

    [Range(0f, 1f)]
    [Tooltip("The overall strength of the vortex swirl.")]
    public float swirlStrength = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("The strength of the pinch/suck-in effect.")]
    public float pinchStrength = 0.3f;
    
    [Range(0.01f, 1f)]
    [Tooltip("The radius of the vortex effect from the center.")]
    public float radius = 0.5f;

    private SpriteRenderer spriteRenderer;

    void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Ensure we have a material instance to modify without changing the original asset
        if (vortexMaterial != null)
        {
            spriteRenderer.material = new Material(vortexMaterial);
        }
    }

    void Update()
    {
        if (spriteRenderer.material != null)
        {
            // Pass the public variables from this script to the shader's properties
            spriteRenderer.material.SetFloat("_SwirlStrength", swirlStrength);
            spriteRenderer.material.SetFloat("_PinchStrength", pinchStrength);
            spriteRenderer.material.SetFloat("_Radius", radius);
        }
    }
}

// -------------------------------------------------------------------
// Shader Code: VortexEffect.shader
// Create a new Unlit Shader and replace its contents with this code.
