// Filename: BallAppearanceController.cs
using Gameplay.BallSystem;
using UnityEngine;

namespace VFX.BallEffects
{
    /// <summary>
    /// Controls the primary appearance of the ball based on its price and scale.
    /// Manages shader properties like orb count, speed, and outline thickness.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class BallVisualController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Ball _ball;

        [Header("Tier 1 Orb Settings")]
        [SerializeField] private float _orbSpeed = 2.0f;
        [SerializeField] private float _baseOrbRadius = 0.04f;

        [Header("Tier 2 Orb Settings")]
        [SerializeField] private int _tier2Threshold = 50;
        [SerializeField] private float _tier2OrbSpeed = -1.0f;
        [SerializeField] private float _tier2BaseOrbRadius = 0.08f;

        [Header("General Appearance")]
        [SerializeField] private float _baseOutlineThickness = 0.05f;
        [SerializeField] private bool _showPathLine = false;
        [SerializeField] private float _basePathLineThickness = 0.005f;

        // --- State & Cache ---
        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;
        private int _lastKnownPrice = -1;
        private float _lastKnownObjectScale = -1f;

        // --- Shader Property IDs ---
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
            if (_ball == null) _ball = GetComponentInParent<Ball>(); //TODO: It's probably bad that I acessing it like this. Oh well..
            _renderer = GetComponent<Renderer>();
            _mpb = new MaterialPropertyBlock();
        }

        private void Update()
        {
            float currentScale = transform.lossyScale.x;
            if (_ball.CurrentPrice != _lastKnownPrice || !Mathf.Approximately(currentScale, _lastKnownObjectScale))
            {
                ApplyVisualsToShader(currentScale);

                _lastKnownPrice = _ball.CurrentPrice;
                _lastKnownObjectScale = currentScale;
            }
        }

        private void ApplyVisualsToShader(float currentScale)
        {
            _renderer.GetPropertyBlock(_mpb);

            int tier1OrbCount = _ball.CurrentPrice;
            int tier2OrbCount = 0;

            if (_tier2Threshold > 0 && _ball.CurrentPrice >= _tier2Threshold)
            {
                tier2OrbCount = _ball.CurrentPrice / _tier2Threshold;
                tier1OrbCount = _ball.CurrentPrice % _tier2Threshold;
            }

            float safeScale = Mathf.Max(currentScale, 0.001f);
            float adjustedOutlineThickness = _baseOutlineThickness / safeScale;
            float adjustedPathThickness = _basePathLineThickness / safeScale;
            float adjustedTier1OrbRadius = _baseOrbRadius / safeScale;
            float adjustedTier2OrbRadius = _tier2BaseOrbRadius / safeScale;

            _mpb.SetColor(BaseColorID, _ball.Color);
            _mpb.SetFloat(OutlineThicknessID, adjustedOutlineThickness);
            _mpb.SetFloat(PathLineThicknessID, adjustedPathThickness);
            _mpb.SetFloat(ShowPathLineID, _showPathLine ? 1.0f : 0.0f);

            _mpb.SetInt(OrbCountID, tier1OrbCount);
            _mpb.SetFloat(OrbSpeedID, _orbSpeed);
            _mpb.SetFloat(OrbRadiusID, adjustedTier1OrbRadius);

            _mpb.SetInt(OrbCountTier2ID, tier2OrbCount);
            _mpb.SetFloat(OrbSpeedTier2ID, _tier2OrbSpeed);
            _mpb.SetFloat(OrbRadiusTier2ID, adjustedTier2OrbRadius);

            _renderer.SetPropertyBlock(_mpb);
        }
    }
}