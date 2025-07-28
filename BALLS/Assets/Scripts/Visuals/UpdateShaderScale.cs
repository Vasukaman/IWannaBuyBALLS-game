using UnityEngine;

/// <summary>
/// This script passes the object's world scale to a material's "_ObjectScale" property.
/// Required for shaders that need to correct for non-uniform scaling.
/// Uses a MaterialPropertyBlock for efficiency, avoiding material duplication.
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class UpdateShaderScale : MonoBehaviour
{
    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    private void OnEnable()
    {
        _renderer = GetComponent<Renderer>();
        _propBlock = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (_renderer == null || _propBlock == null)
        {
            OnEnable(); // Re-initialize if needed
        }

        // Get the current values from the material
        _renderer.GetPropertyBlock(_propBlock);

        // Set the scale vector
        _propBlock.SetVector("_ObjectScale", transform.lossyScale);

        // Apply the property block back to the renderer
        _renderer.SetPropertyBlock(_propBlock);
    }
}