using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Ball), typeof(Renderer))]
public class BallVisualController : MonoBehaviour
{
    private const int MAX_NEIGHBORS = 10; // Must match the shader's array size

    [Header("Tier 1 Orb Settings")]
    [SerializeField] private float orbSpeed = 2.0f;
    [SerializeField] private bool showPathLine = false;
    [SerializeField] private float baseOrbRadius = 0.04f;

    [Header("Tier 2 Orb Settings")]
    [Tooltip("The price at which Tier 2 orbs start appearing.")]
    [SerializeField] private int tier2Threshold = 50;
    [Tooltip("The speed of the Tier 2 orbs.")]
    [SerializeField] private float tier2OrbSpeed = -1.0f;
    [Tooltip("The size of the Tier 2 orbs, independent of scale.")]
    [SerializeField] private float tier2BaseOrbRadius = 0.08f;


    [Header("Scale-Independent Sizes")]
    [SerializeField] private float baseOutlineThickness = 0.05f;
    [SerializeField] private float basePathLineThickness = 0.005f;

    [Header("Neighbor Proximity Effect")]
    [Tooltip("How far to look for neighbors to blend colors with.")]
    [SerializeField] private float neighborDetectionRadius = 2.0f;
    [Tooltip("How smooth the color transition is. Higher is sharper.")]
    [SerializeField] private float proximityBlendSharpness = 2.0f;

    // --- Component & Data References ---
    [SerializeField] private Ball _ball;
    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;

    // --- Neighbor Data Arrays ---
    private readonly Vector4[] _neighborPositions = new Vector4[MAX_NEIGHBORS];
    private readonly Vector4[] _neighborColorsAsVectors = new Vector4[MAX_NEIGHBORS];
    private static readonly Collider2D[] _neighborResults = new Collider2D[MAX_NEIGHBORS + 1];

    // --- State Tracking ---
    private int _lastKnownPrice = -1;
    private float _lastKnownObjectScale = -1f;

    // --- Shader Property IDs ---
    private static readonly int BaseColorID = Shader.PropertyToID("_Color");
    private static readonly int OutlineThicknessID = Shader.PropertyToID("_OutlineThickness");
    // Tier 1
    private static readonly int OrbCountID = Shader.PropertyToID("_OrbCount");
    private static readonly int OrbSpeedID = Shader.PropertyToID("_OrbSpeed");
    private static readonly int OrbRadiusID = Shader.PropertyToID("_OrbRadius");
    // Tier 2
    private static readonly int OrbCountTier2ID = Shader.PropertyToID("_OrbCountTier2");
    private static readonly int OrbSpeedTier2ID = Shader.PropertyToID("_OrbSpeedTier2");
    private static readonly int OrbRadiusTier2ID = Shader.PropertyToID("_OrbRadiusTier2");
    // Paths
    private static readonly int ShowPathLineID = Shader.PropertyToID("_ShowPathLine");
    private static readonly int PathLineThicknessID = Shader.PropertyToID("_PathLineThickness");
    // Neighbors
    private static readonly int NeighborPositionsID = Shader.PropertyToID("_NeighborPositions");
    private static readonly int NeighborColorsID = Shader.PropertyToID("_NeighborColors");
    private static readonly int NeighborCountID = Shader.PropertyToID("_NeighborCount");
    private static readonly int ProximityBlendSharpnessID = Shader.PropertyToID("_ProximityBlendSharpness");


    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
    }

    void Update()
    {
        FindAndSendNeighborData();
        ApplyVisualsToShader();
    }

    private void FindAndSendNeighborData()
    {
        int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, neighborDetectionRadius, _neighborResults);
        int neighborCount = 0;
        for (int i = 0; i < hitCount && neighborCount < MAX_NEIGHBORS; i++)
        {
            if (_neighborResults[i].gameObject == gameObject) continue;
            if (_neighborResults[i].TryGetComponent<Ball>(out var neighborBall))
            {
                Transform neighborTransform = neighborBall.transform;
                _neighborPositions[neighborCount] = new Vector4(
                    neighborTransform.position.x,
                    neighborTransform.position.y,
                    neighborTransform.lossyScale.x * 0.5f,
                    0
                );
                _neighborColorsAsVectors[neighborCount] = neighborBall.color;
                neighborCount++;
            }
        }

        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetInt(NeighborCountID, neighborCount);
        if (neighborCount > 0)
        {
            _mpb.SetVectorArray(NeighborPositionsID, _neighborPositions);
            _mpb.SetVectorArray(NeighborColorsID, _neighborColorsAsVectors);
        }
        _renderer.SetPropertyBlock(_mpb);
    }


    private void ApplyVisualsToShader()
    {
        _renderer.GetPropertyBlock(_mpb);

        float currentScale = Mathf.Max(transform.lossyScale.x, 0.001f);
        if (_ball.CurrentPrice != _lastKnownPrice || currentScale != _lastKnownObjectScale)
        {
            // --- Orb Distribution Logic ---
            int tier1OrbCount = _ball.CurrentPrice;
            int tier2OrbCount = 0;

            if (tier2Threshold > 0 && _ball.CurrentPrice >= tier2Threshold)
            {
                tier2OrbCount = _ball.CurrentPrice / tier2Threshold;
                tier1OrbCount = _ball.CurrentPrice % tier2Threshold;
            }

            // --- Adjust sizes for object scale ---
            float adjustedOutlineThickness = baseOutlineThickness / currentScale;
            float adjustedPathThickness = basePathLineThickness / currentScale;
            float adjustedTier1OrbRadius = baseOrbRadius / currentScale;
            float adjustedTier2OrbRadius = tier2BaseOrbRadius / currentScale;

            // --- Set Shader Properties ---
            _mpb.SetColor(BaseColorID, _ball.color);
            _mpb.SetFloat(OutlineThicknessID, adjustedOutlineThickness);
            _mpb.SetFloat(PathLineThicknessID, adjustedPathThickness);
            _mpb.SetFloat(ProximityBlendSharpnessID, proximityBlendSharpness);
            _mpb.SetFloat(ShowPathLineID, showPathLine ? 1.0f : 0.0f);

            // Tier 1
            _mpb.SetInt(OrbCountID, tier1OrbCount);
            _mpb.SetFloat(OrbSpeedID, orbSpeed);
            _mpb.SetFloat(OrbRadiusID, adjustedTier1OrbRadius);

            // Tier 2
            _mpb.SetInt(OrbCountTier2ID, tier2OrbCount);
            _mpb.SetFloat(OrbSpeedTier2ID, tier2OrbSpeed);
            _mpb.SetFloat(OrbRadiusTier2ID, adjustedTier2OrbRadius);

            _lastKnownPrice = _ball.CurrentPrice;
            _lastKnownObjectScale = currentScale;
        }

        _renderer.SetPropertyBlock(_mpb);
    }
}
