using UnityEngine;

// This attribute ensures the script is always attached to a camera
[RequireComponent(typeof(Camera))]
// This attribute makes the script run in the editor, so you can see the effect without playing the game
[ExecuteInEditMode]
public class PixelateDissolveEffect : MonoBehaviour
{
    // Public variable to assign the shader in the Inspector
    public Shader effectShader;

    // Public variables to control the effect's parameters from the Inspector
    [Range(1, 512)]
    public float pixelSize = 100f;

    [Range(0.0f, 1.0f)]
    public float dissolveThreshold = 0.5f;

    public Color dissolveColor = Color.black;

    // Private variable to hold the material created from the shader
    private Material effectMaterial;

    void OnEnable()
    {
        // Check if the shader is assigned
        if (effectShader == null)
        {
            Debug.LogError("Shader not assigned to the PixelateDissolveEffect script!");
            enabled = false;
            return;
        }

        // Create a new material with the assigned shader
        effectMaterial = new Material(effectShader);
    }

    void OnDisable()
    {
        // Clean up the material when the script is disabled or the object is destroyed
        if (effectMaterial != null)
        {
            DestroyImmediate(effectMaterial);
        }
    }

    // This method is called by Unity for every frame that is rendered
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // If the material is not created, just pass the original image through
        if (effectMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        // Pass the public variables from this script to the shader's properties
        effectMaterial.SetFloat("_PixelSize", pixelSize);
        effectMaterial.SetFloat("_DissolveThreshold", dissolveThreshold);
        effectMaterial.SetColor("_DissolveColor", dissolveColor);

        // Apply the shader to the source image and output to the destination
        Graphics.Blit(source, destination, effectMaterial);
    }
}
