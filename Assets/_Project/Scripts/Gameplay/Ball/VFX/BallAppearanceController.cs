// Filename: BallAppearanceController.cs
using Gameplay.BallSystem;
using UnityEngine;

namespace VFX.BallEffects
{
    /// <summary>
    /// Controls the primary appearance of the ball based on its price and scale.
    /// It listens for events and updates the shader properties accordingly.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class BallAppearanceController : MonoBehaviour
    {
     
        // --- References ---
        private BallView _ballView;
        private BallAppearanceProfile _appearanceProfile;


        // --- State & Cache ---
        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        private float _lastKnownScale = -1f;


        // --- Shader Contract ---
        // This component fulfills a contract with a shader that expects the following properties:
        // - fixed4 _Color;
        // - float _OutlineThickness;
        // - int _OrbCount;
        // - float _OrbSpeed;
        // - float _OrbRadius;
        // - int _OrbCountTier2;
        // - float _OrbSpeedTier2;
        // - float _OrbRadiusTier2;
        // - float _ShowPathLine;
        // - float _PathLineThickness;
        private static readonly int BaseColorID = Shader.PropertyToID("_Color");
        private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
        private static readonly int OrbCountID = Shader.PropertyToID("_OrbCount");
        private static readonly int OrbSpeedID = Shader.PropertyToID("_OrbSpeed");
        private static readonly int OrbRadiusID = Shader.PropertyToID("_OrbRadius");
        private static readonly int OrbCountTier2ID = Shader.PropertyToID("_OrbCountTier2");
        private static readonly int OrbSpeedTier2ID = Shader.PropertyToID("_OrbSpeedTier2");
        private static readonly int OrbRadiusTier2ID = Shader.PropertyToID("_OrbRadiusTier2");
        private static readonly int ShowPathLineID = Shader.PropertyToID("_ShowPathLine");
        private static readonly int PathLineThicknessID = Shader.PropertyToID("_PathLineThickness");

        private void Awake()
        {
            // The specialist is responsible for finding its parent/root.
            _ballView = GetComponentInParent<BallView>();

            if (_ballView == null)
            {
                Debug.LogError("BallAppearanceController must be a child of a BallView.", this);
                enabled = false;
                return;
            }

            _appearanceProfile = _ballView.Profile.Appearance;

            if (_appearanceProfile == null)
            {
                Debug.LogError("BallAppearanceProfile is not assigned in the master BallProfile!", this);
                enabled = false;
                return;
            }
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (_ballView == null) return;

            // Subscribe to the View's lifecycle event
            _ballView.OnInitialize += HandleBallInitialized;

            // Subscribe to the Model's data event
            if (_ballView.Data != null)
            {
                _ballView.Data.OnPriceChanged += HandlePriceChanged;
            }
        }

        /// <summary>
        /// This public method allows the parent (BallView) to provide its reference.
        /// </summary>

        private void OnDisable()
        {
            if (_ballView == null) return;
            _ballView.OnInitialize -= HandleBallInitialized;
            if (_ballView.Data != null)
            {
                _ballView.Data.OnPriceChanged -= HandlePriceChanged;
            }
        }

        // The Update loop is now only for checking scale, which is a transform property.
        private void Update()
        {
            // We still need to check the scale every frame, as there's no "OnScaleChanged" event.
            float currentScale = transform.lossyScale.x;
            if (!Mathf.Approximately(currentScale, _lastKnownScale))
            {
                ApplyVisualsToShader();
            }
        }

        private void HandleBallInitialized(BallView ball)
        {
            // Ensure we are subscribed to the new Data model's events
            if (ball.Data != null)
            {
                ball.Data.OnPriceChanged -= HandlePriceChanged;
                ball.Data.OnPriceChanged += HandlePriceChanged;
            }
            ApplyVisualsToShader();
        }

        private void HandlePriceChanged(int newPrice)
        {
            ApplyVisualsToShader();
        }

        /// <summary>
        /// Calculates and applies all appearance-related properties to the shader.
        /// This is now called by events instead of being polled in Update.
        /// </summary>
        private void ApplyVisualsToShader()
        {
            if (_ballView == null || _ballView.Data == null) return;

            _renderer.GetPropertyBlock(_propertyBlock);

            int currentPrice = _ballView.Data.CurrentPrice;
            float currentScale = transform.lossyScale.x;
            _lastKnownScale = currentScale; // Cache the scale

            // --- Orb Distribution ---
            int tier1OrbCount = currentPrice;
            int tier2OrbCount = 0;
            if (_appearanceProfile.Tier2Threshold > 0 && currentPrice >= _appearanceProfile.Tier2Threshold)
            {
                tier2OrbCount = currentPrice / _appearanceProfile.Tier2Threshold;
                tier1OrbCount = currentPrice % _appearanceProfile.Tier2Threshold;
            }

            // --- Scale Compensation ---
            float safeScale = Mathf.Max(currentScale, 0.001f);
            float adjustedOutlineThickness = _appearanceProfile.BaseOutlineThickness / safeScale;
            float adjustedPathThickness = _appearanceProfile.BasePathLineThickness / safeScale;
            float adjustedTier1OrbRadius = _appearanceProfile.BaseOrbRadius / safeScale;
            float adjustedTier2OrbRadius = _appearanceProfile.Tier2BaseOrbRadius / safeScale;

            // --- Set Shader Properties ---
            _propertyBlock.SetColor(BaseColorID, _ballView.Color);
            _propertyBlock.SetFloat(OutlineThicknessID, adjustedOutlineThickness);
            _propertyBlock.SetFloat(PathLineThicknessID, adjustedPathThickness);
            _propertyBlock.SetFloat(ShowPathLineID, _appearanceProfile.ShowPathLine ? 1.0f : 0.0f);

            _propertyBlock.SetInt(OrbCountID, tier1OrbCount);
            _propertyBlock.SetFloat(OrbSpeedID, _appearanceProfile.OrbSpeed);
            _propertyBlock.SetFloat(OrbRadiusID, adjustedTier1OrbRadius);

            _propertyBlock.SetInt(OrbCountTier2ID, tier2OrbCount);
            _propertyBlock.SetFloat(OrbSpeedTier2ID, _appearanceProfile.Tier2OrbSpeed);
            _propertyBlock.SetFloat(OrbRadiusTier2ID, adjustedTier2OrbRadius);

            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}