// Filename: SellZoneView.cs
using Gameplay.Gadgets;
using UnityEngine;

namespace VFX.Gadgets
{
    [RequireComponent(typeof(Renderer))]
    public class SellZoneView : MonoBehaviour
    {

        private SellZoneViewProfile _profile;

        // Public accessor for the Presenter to read the acceleration value.
        public float SpeedAcceleration => _profile.SpeedAcceleration;

        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        
        // Cached original values from the material
        private Color _originalDashColor, _originalLineColor;
        private Vector2 _originalLineSpeed;
        private float _originalLineThickness, _originalLineSpacing;

        // Shader Property IDs
        private static readonly int DashColorID = Shader.PropertyToID("_DashColor");
        private static readonly int LineColorID = Shader.PropertyToID("_LineColor");
        private static readonly int LineScrollSpeedID = Shader.PropertyToID("_LineScrollSpeed");
        private static readonly int LineThicknessID = Shader.PropertyToID("_LineThickness");
        private static readonly int LineSpacingID = Shader.PropertyToID("_LineSpacing");
        private static readonly int ObjectScaleID = Shader.PropertyToID("_ObjectScale");

        private void Awake()
        {
            // It pulls the master profile from the Presenter...
            var presenter = GetComponent<GadgetSellZonePresenter>();
            if (presenter != null)
            {
                _profile = presenter.Profile.View;
            }

            if (_profile == null)
            {
                Debug.LogError("Could not find a valid SellZoneViewProfile! Disabling component.", this);
                enabled = false;
                return;
            }

            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            // Apply the default appearance as soon as the game starts.
            SetDefaultAppearance();
        }

        private void Update()
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector("_ObjectScale", transform.lossyScale);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public void SetDefaultAppearance()
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            // Read all default values directly from the profile.
            _propertyBlock.SetColor(DashColorID, _profile.DefaultDashColor);
            _propertyBlock.SetColor(LineColorID, _profile.DefaultLineColor);
            _propertyBlock.SetVector(LineScrollSpeedID, _profile.DefaultLineSpeed);
            _propertyBlock.SetFloat(LineThicknessID, _profile.DefaultLineThickness);
            _propertyBlock.SetFloat(LineSpacingID, _profile.DefaultLineSpacing);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        public void UpdateSellingAppearance(float progress, float speedFactor)
        {
            _renderer.GetPropertyBlock(_propertyBlock);

            float currentScale = Mathf.Max(transform.lossyScale.x, 0.001f);

            // Lerp from the profile's default values to the profile's selling values.
            float adjustedThickness = Mathf.Lerp(_profile.DefaultLineThickness, _profile.SellingLineThickness, progress) / currentScale;
            float adjustedSpacing = Mathf.Lerp(_profile.DefaultLineSpacing, _profile.SellingLineSpacing, progress) / currentScale;


            //We can lerp the colors but it doesn't look that good.
            //Color currentDashColor = Color.Lerp(_originalDashColor, _profile.SellingDashColor, progress);
            //Color currentLineColor = Color.Lerp(_originalLineColor, _profile.SellingLineColor, progress);

            _propertyBlock.SetColor(DashColorID, _profile.SellingDashColor);
            _propertyBlock.SetColor(LineColorID, _profile.SellingLineColor);
            _propertyBlock.SetVector(LineScrollSpeedID, _profile.DefaultLineSpeed * speedFactor);
            _propertyBlock.SetFloat(LineThicknessID, adjustedThickness);
            _propertyBlock.SetFloat(LineSpacingID, adjustedSpacing);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        // The CacheOriginalProperties method is now completely GONE.
    }
}