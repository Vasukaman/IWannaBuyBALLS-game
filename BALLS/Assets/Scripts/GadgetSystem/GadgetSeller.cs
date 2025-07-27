using System.Collections;
using UnityEngine;
//using Game.Gameplay;

/// <summary>
/// Manages a zone where IGadgetSellable items can be sold.
/// Provides visual feedback ON ITSELF and handles the selling process over time.
/// </summary>
[RequireComponent(typeof(BoxCollider2D), typeof(Renderer))]
public class GadgetSeller : MonoBehaviour
{
    [Header("Selling Configuration")]
    [Tooltip("The time in seconds a gadget must be fully in the zone to be sold.")]
    [SerializeField] private float _timeToSell = 3.0f;
    [Tooltip("How much the line scroll speed increases each second.")]
    [SerializeField] private float _speedAcceleration = 1.5f;

    [Header("Visuals - Selling State")]
    [Tooltip("The color of the outline when a gadget is being sold.")]
    [SerializeField] private Color _sellingDashColor = Color.green;
    [Tooltip("The color of the interior lines when a gadget is being sold.")]
    [SerializeField] private Color _sellingLineColor = new Color(0, 1, 0, 0.25f);

    // --- Private State ---
    private BoxCollider2D _sellZoneCollider;
    private Renderer _zoneRenderer;
    private Coroutine _sellingCoroutine;
    private IGadgetSellable _currentTarget;
    private MaterialPropertyBlock _propertyBlock;

    // --- Stored Original Material Properties ---
    private Color _originalDashColor;
    private Color _originalLineColor;
    private Vector2 _originalLineSpeed;

    // --- Shader Property IDs ---
    private static readonly int DashColorID = Shader.PropertyToID("_DashColor");
    private static readonly int LineColorID = Shader.PropertyToID("_LineColor");
    private static readonly int LineScrollSpeedID = Shader.PropertyToID("_LineScrollSpeed");
    private static readonly int ObjectScaleID = Shader.PropertyToID("_ObjectScale");

    private void Awake()
    {
        _sellZoneCollider = GetComponent<BoxCollider2D>();
        _zoneRenderer = GetComponent<Renderer>();
        _sellZoneCollider.isTrigger = true;
        _propertyBlock = new MaterialPropertyBlock();

        // Store the zone's own initial material state so we can revert to it.
        var initialMaterial = _zoneRenderer.material; // Creates an instance
        _originalDashColor = initialMaterial.GetColor(DashColorID);
        _originalLineColor = initialMaterial.GetColor(LineColorID);
        _originalLineSpeed = initialMaterial.GetVector(LineScrollSpeedID);

        // Pass the object's scale to the shader for correct calculations
        _propertyBlock.SetVector(ObjectScaleID, transform.lossyScale);
        _zoneRenderer.SetPropertyBlock(_propertyBlock);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (_sellingCoroutine != null) return;

        if (other.TryGetComponent<IGadgetSellable>(out var gadget))
        {
            if (IsFullyContained(other))
            {
                _currentTarget = gadget;
                _sellingCoroutine = StartCoroutine(SellProcessCoroutine());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_currentTarget != null && other.gameObject == _currentTarget.Instance)
        {
            StopSellingProcess();
        }
    }

    /// <summary>
    /// Coroutine that handles the timed selling process and visual feedback on this zone.
    /// </summary>
    private IEnumerator SellProcessCoroutine()
    {
        float elapsedTime = 0f;

        // --- Activate Selling Visuals ---
        _zoneRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(DashColorID, _sellingDashColor);
        _propertyBlock.SetColor(LineColorID, _sellingLineColor);
        _zoneRenderer.SetPropertyBlock(_propertyBlock);

        // --- Main Loop ---
        while (elapsedTime < _timeToSell)
        {
            if (_currentTarget == null || !_currentTarget.Instance.activeInHierarchy || !IsFullyContained(_currentTarget.ObjectCollider))
            {
                StopSellingProcess();
                yield break;
            }

            elapsedTime += Time.deltaTime;

            float accelerationFactor = Mathf.Lerp(1, _speedAcceleration, elapsedTime / _timeToSell);
            //= 1.0f + (elapsedTime/ _timeToSell * _speedAcceleration);
            Vector2 currentLineSpeed = _originalLineSpeed * accelerationFactor;

            // Update shader properties on this object's renderer
            _propertyBlock.SetVector(LineScrollSpeedID, currentLineSpeed);
            _zoneRenderer.SetPropertyBlock(_propertyBlock);

            yield return null;
        }

        // --- Finalization ---
        _currentTarget.Sell();
        StopSellingProcess(); // Revert visuals and clear state
    }

    /// <summary>
    /// Stops the current selling process and resets the zone's visuals and state.
    /// </summary>
    private void StopSellingProcess()
    {
        if (_sellingCoroutine != null)
        {
            StopCoroutine(_sellingCoroutine);
        }

        // Revert this zone's material to its original state
        _zoneRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(DashColorID, _originalDashColor);
        _propertyBlock.SetColor(LineColorID, _originalLineColor);
        _propertyBlock.SetVector(LineScrollSpeedID, _originalLineSpeed);
        _zoneRenderer.SetPropertyBlock(_propertyBlock);

        // Clear state
        _sellingCoroutine = null;
        _currentTarget = null;
    }

    /// <summary>
    /// Checks if another collider is fully contained within this trigger zone.
    /// </summary>
    private bool IsFullyContained(Collider2D otherCollider)
    {
        return true;
        if (otherCollider == null) return false;

        Bounds otherBounds = otherCollider.bounds;
        Bounds zoneBounds = _sellZoneCollider.bounds;

        return zoneBounds.Contains(otherBounds.min) && zoneBounds.Contains(otherBounds.max);
    }
}
