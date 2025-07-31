// Filename: SellZoneView.cs
using UnityEngine;

namespace VFX.Gadgets
{
    [RequireComponent(typeof(Renderer))]
    public class SellZoneView : MonoBehaviour
    {
        [Header("Selling State Visuals")]
        [SerializeField] private Color _sellingDashColor = Color.green;
        [SerializeField] private Color _sellingLineColor = new Color(0, 1, 0, 0.25f);
        [SerializeField] private float _sellingLineThickness = 3.0f; // Your value of 3
        [SerializeField] private float _sellingLineSpacing = 0.05f;
        [Tooltip("How much the line scroll speed increases each second.")]
        [SerializeField] private float _speedAcceleration = 3.0f; // Setting moved here!

        public float SpeedAcceleration => _speedAcceleration; // Public accessor for the Presenter

        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        private Color _originalDashColor, _originalLineColor;
        private Vector2 _originalLineSpeed;
        private float _originalLineThickness, _originalLineSpacing;

        private static readonly int DashColorID = Shader.PropertyToID("_DashColor");
        private static readonly int LineColorID = Shader.PropertyToID("_LineColor");
        private static readonly int LineScrollSpeedID = Shader.PropertyToID("_LineScrollSpeed");
        private static readonly int LineThicknessID = Shader.PropertyToID("_LineThickness");
        private static readonly int LineSpacingID = Shader.PropertyToID("_LineSpacing");
        private static readonly int ObjectScaleID = Shader.PropertyToID("_ObjectScale");

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            CacheOriginalProperties();
        }
        
        private void Update()
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(ObjectScaleID, transform.lossyScale);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetDefaultAppearance()
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(DashColorID, _originalDashColor);
            _propertyBlock.SetColor(LineColorID, _originalLineColor);
            _propertyBlock.SetVector(LineScrollSpeedID, _originalLineSpeed);
            _propertyBlock.SetFloat(LineThicknessID, _originalLineThickness);
            _propertyBlock.SetFloat(LineSpacingID, _originalLineSpacing);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public void UpdateSellingAppearance(float progress, float speedFactor)
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            
       
            float currentScale = Mathf.Max(transform.lossyScale.x, 0.001f);
            float adjustedThickness = Mathf.Lerp(_originalLineThickness, _sellingLineThickness, progress) / currentScale;
            float adjustedSpacing = Mathf.Lerp(_originalLineSpacing, _sellingLineSpacing, progress) / currentScale;

            _propertyBlock.SetColor(DashColorID, _sellingDashColor);
            _propertyBlock.SetColor(LineColorID, _sellingLineColor);
            _propertyBlock.SetVector(LineScrollSpeedID, _originalLineSpeed * speedFactor);
            _propertyBlock.SetFloat(LineThicknessID, adjustedThickness);
            _propertyBlock.SetFloat(LineSpacingID, adjustedSpacing);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        private void CacheOriginalProperties()
        {
            var initialMaterial = _renderer.material;
            _originalDashColor = initialMaterial.GetColor(DashColorID);
            _originalLineColor = initialMaterial.GetColor(LineColorID);
            _originalLineSpeed = initialMaterial.GetVector(LineScrollSpeedID);
            _originalLineThickness = initialMaterial.GetFloat(LineThicknessID);
            _originalLineSpacing = initialMaterial.GetFloat(LineSpacingID);
        }
    }
}