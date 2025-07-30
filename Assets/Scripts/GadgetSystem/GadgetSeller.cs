using System.Collections;
using UnityEngine;
//using Game.Gameplay;

/// <summary>
/// Manages a zone where IGadgetSellable items can be sold.
/// The zone continuously and dynamically resizes with a wobbly effect to track a gadget,
/// then begins the selling process with visual feedback on itself.
/// </summary>
/// 


//This is definetly a script I need to break into smaller ones. 
//I'm leaving proper refactoring until then.
[RequireComponent(typeof(BoxCollider2D), typeof(Renderer))]
public class GadgetSeller : MonoBehaviour
{
    [Header("Selling Configuration")]
    [Tooltip("The time in seconds a gadget must be fully in the zone to be sold.")]
    [SerializeField] private float _timeToSell = 3.0f;
    [Tooltip("How much the line scroll speed increases each second.")]
    [SerializeField] private float _speedAcceleration = 1.5f;

    [Header("Wobble Animation")]
    [Tooltip("How 'springy' the resize animation is. Higher values are faster and more bouncy.")]
    [SerializeField] private float _stiffness = 20f;
    [Tooltip("How much the wobble is dampened. 0 is no damping (infinite wobble), 1 is critical damping (no wobble).")]
    [Range(0, 1)]
    [SerializeField] private float _damping = 0.7f;
    [Tooltip("The maximum distance from the original corner that the zone will track a gadget before resetting.")]
    [SerializeField] private float _maxTrackingDistance = 10f;
    [Tooltip("How much extra space to add around the gadget when capturing it.")]
    [SerializeField] private float _capturePadding = 0.5f;

    [Header("Visuals - Selling State")]
    [Tooltip("The color of the outline when a gadget is being sold.")]
    [SerializeField] private Color _sellingDashColor = Color.green;
    [Tooltip("The color of the interior lines when a gadget is being sold.")]
    [SerializeField] private Color _sellingLineColor = new Color(0, 1, 0, 0.25f);
    [Tooltip("The maximum thickness of the interior lines during a sale.")]
    [SerializeField] private float _sellingLineThickness = 0.1f;
    [Tooltip("The minimum spacing of the interior lines during a sale.")]
    [SerializeField] private float _sellingLineSpacing = 0.05f;


    // --- Private State ---
    private BoxCollider2D _sellZoneCollider;
    private Renderer _zoneRenderer;
    private Coroutine _sellingCoroutine;
    private IGadgetSellable _currentTarget;
    private MaterialPropertyBlock _propertyBlock;

    // --- Transform & Material State ---
    private Vector3 _originalPosition;
    private Vector3 _originalScale;
    private Color _originalDashColor;
    private Color _originalLineColor;
    private Vector2 _originalLineSpeed;
    private float _originalLineThickness;
    private float _originalLineSpacing;

    // --- Spring Animation State ---
    private Vector3 _positionVelocity;
    private Vector3 _scaleVelocity;

    // --- Shader Property IDs ---
    private static readonly int DashColorID = Shader.PropertyToID("_DashColor");
    private static readonly int LineColorID = Shader.PropertyToID("_LineColor");
    private static readonly int LineScrollSpeedID = Shader.PropertyToID("_LineScrollSpeed");
    private static readonly int LineThicknessID = Shader.PropertyToID("_LineThickness");
    private static readonly int LineSpacingID = Shader.PropertyToID("_LineSpacing");
    private static readonly int ObjectScaleID = Shader.PropertyToID("_ObjectScale");

    private void Awake()
    {
        _sellZoneCollider = GetComponent<BoxCollider2D>();
        _zoneRenderer = GetComponent<Renderer>();
        _sellZoneCollider.isTrigger = true;
        _propertyBlock = new MaterialPropertyBlock();

        // --- Store Initial State ---
        _originalPosition = transform.position;
        _originalScale = transform.localScale;
        var initialMaterial = _zoneRenderer.material;
        _originalDashColor = initialMaterial.GetColor(DashColorID);
        _originalLineColor = initialMaterial.GetColor(LineColorID);
        _originalLineSpeed = initialMaterial.GetVector(LineScrollSpeedID);
        _originalLineThickness = initialMaterial.GetFloat(LineThicknessID);
        _originalLineSpacing = initialMaterial.GetFloat(LineSpacingID);
    }

    private void Update()
    {
        // Default to the original state.
        Vector3 targetPosition = _originalPosition;
        Vector3 targetScale = _originalScale;

        // If we have a target, check if it's still valid and in range.
        if (_currentTarget != null)
        {
            Vector3 anchorCorner = new Vector3(_originalPosition.x + _originalScale.x / 2, _originalPosition.y + _originalScale.y / 2, 0);
            // Use ClosestPoint for a more accurate distance check to the gadget's edge.
            float distance = Vector3.Distance(anchorCorner, _currentTarget.ObjectCollider.bounds.ClosestPoint(anchorCorner));

            if (distance <= _maxTrackingDistance)
            {
                // Target is valid and in range, so we calculate the new target transform.
                Vector3 padding = new Vector3(_capturePadding, _capturePadding, 0);
                Vector3 targetCorner = _currentTarget.ObjectCollider.bounds.min - padding;
                targetScale = new Vector3(anchorCorner.x - targetCorner.x, anchorCorner.y - targetCorner.y, _originalScale.z);
                targetPosition = new Vector3(targetCorner.x + targetScale.x / 2.0f, targetCorner.y + targetScale.y / 2.0f, _originalPosition.z);
            }
            else
            {
                // Target is out of range, stop tracking it. The animation will return to the original state.
                StopAllProcesses();
            }
        }

        // --- SPRING ANIMATION LOGIC ---
        // This logic runs every frame, smoothly moving towards the current target (either the gadget or the original state).
        Vector3 positionForce = (targetPosition - transform.position) * _stiffness;
        _positionVelocity = (_positionVelocity + positionForce * Time.deltaTime) * (1f - _damping);
        transform.position += _positionVelocity * Time.deltaTime;

        Vector3 scaleForce = (targetScale - transform.localScale) * _stiffness;
        _scaleVelocity = (_scaleVelocity + scaleForce * Time.deltaTime) * (1f - _damping);
        transform.localScale += _scaleVelocity * Time.deltaTime;

        UpdateShaderScale();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_currentTarget != null) return; // Already tracking something.
        if (other.TryGetComponent<IGadgetSellable>(out var gadget))
        {
            // Only start tracking if the gadget enters within the max distance.
            Vector3 anchorCorner = new Vector3(_originalPosition.x + _originalScale.x / 2, _originalPosition.y + _originalScale.y / 2, 0);
            float distance = Vector3.Distance(anchorCorner, gadget.ObjectCollider.bounds.ClosestPoint(anchorCorner));

            if (distance <= _maxTrackingDistance)
            {
                _currentTarget = gadget;
                if (_sellingCoroutine != null)
                {
                    StopCoroutine(_sellingCoroutine);
                }
                _sellingCoroutine = StartCoroutine(SellProcessCoroutine());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_currentTarget != null && other.gameObject == _currentTarget.Instance)
        {
            StopAllProcesses();
        }
    }

    /// <summary>
    /// Coroutine that handles the timed selling process and visual feedback.
    /// </summary>
    private IEnumerator SellProcessCoroutine()
    {
        // Wait until the spring animation has fully captured the gadget.
        while (_currentTarget != null && !IsFullyContained(_currentTarget.ObjectCollider))
        {
            yield return null;
        }

        // If the target was lost while we were waiting, exit.
        if (_currentTarget == null) yield break;

        // --- Activate Selling Visuals ---
        _zoneRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(DashColorID, _sellingDashColor);
        _propertyBlock.SetColor(LineColorID, _sellingLineColor);
        _zoneRenderer.SetPropertyBlock(_propertyBlock);

        // --- Main Selling Loop ---
        float elapsedTime = 0f;
        while (elapsedTime < _timeToSell)
        {
            if (_currentTarget == null || !_currentTarget.Instance.activeInHierarchy)
            {
                StopAllProcesses();
                yield break;
            }

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / _timeToSell;

            // Animate line speed
            float accelerationFactor = 1.0f + (elapsedTime * _speedAcceleration);
            Vector2 currentLineSpeed = _originalLineSpeed * accelerationFactor;

            // Animate line thickness
            float currentLineThickness = Mathf.Lerp(_originalLineThickness, _sellingLineThickness, progress);

            // Animate line spacing
            float currentLineSpacing = Mathf.Lerp(_originalLineSpacing, _sellingLineSpacing, progress);

            _propertyBlock.SetFloat(LineThicknessID, currentLineThickness);
            _propertyBlock.SetFloat(LineSpacingID, currentLineSpacing);
            _propertyBlock.SetVector(LineScrollSpeedID, currentLineSpeed);
            _zoneRenderer.SetPropertyBlock(_propertyBlock);

            yield return null;
        }

        // --- Finalization: Sell the item ---
        if (_currentTarget != null)
        {
            _currentTarget.Sell();
        }

        StopAllProcesses();
    }

    /// <summary>
    /// Stops the selling process and clears the target, allowing the Update loop to reset the zone.
    /// </summary>
    private void StopAllProcesses()
    {
        if (_sellingCoroutine != null)
        {
            StopCoroutine(_sellingCoroutine);
            _sellingCoroutine = null;
        }

        // Revert material properties immediately.
        _zoneRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetColor(DashColorID, _originalDashColor);
        _propertyBlock.SetColor(LineColorID, _originalLineColor);
        _propertyBlock.SetVector(LineScrollSpeedID, _originalLineSpeed);
        _propertyBlock.SetFloat(LineThicknessID, _originalLineThickness);
        _propertyBlock.SetFloat(LineSpacingID, _originalLineSpacing);
        _zoneRenderer.SetPropertyBlock(_propertyBlock);

        _currentTarget = null;
    }

    /// <summary>
    /// Checks if another collider is fully contained within this trigger zone.
    /// </summary>
    private bool IsFullyContained(Collider2D otherCollider)
    {
        return true;
        if (otherCollider == null) return false;
        return _sellZoneCollider.bounds.Contains(otherCollider.bounds.min) &&
               _sellZoneCollider.bounds.Contains(otherCollider.bounds.max);
    }

    /// <summary>
    /// Helper method to update the shader's scale property.
    /// </summary>
    private void UpdateShaderScale()
    {
        _zoneRenderer.GetPropertyBlock(_propertyBlock);
        _propertyBlock.SetVector(ObjectScaleID, transform.lossyScale);
        _zoneRenderer.SetPropertyBlock(_propertyBlock);
    }
}
