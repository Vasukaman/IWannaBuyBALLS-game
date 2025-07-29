//using Reflex.Attributes;
//using System.Linq;
//using UnityEngine;

//[RequireComponent(typeof(Renderer))]
//public class ProximityEffectController : MonoBehaviour
//{
//    [Header("Proximity Settings")]
//    [SerializeField] private float proximityRadius = 1f;
//    [SerializeField] private Color proximityColor = Color.yellow;
//    [SerializeField] private float maxProximityStrength = 0.5f;
//    [SerializeField] private int maxNeighbors = 3;

//    [SerializeField] private Ball ball;

//    private Renderer _renderer;
//    private MaterialPropertyBlock _props;
//    [SerializeField] private readonly Vector4[] _neighborPositions = new Vector4[10];//TODO WTF IS THIS WE CANT CHANGE MAX WITHOUT CHANGING THIS
//    private readonly float[] _neighborStrengths = new float[10];

//    private void Awake()
//    {
//        _renderer = GetComponent<Renderer>();
//        _props = new MaterialPropertyBlock();
//    }

//    private void LateUpdate()
//    {
//        if (ball.BallFactory == null) return;

//        UpdateProximityData();
//        UpdateMaterialProperties();
//    }

//    private void UpdateProximityData()
//    {
//        // Reset neighbor data
//        System.Array.Clear(_neighborPositions, 0, _neighborPositions.Length);
//        System.Array.Clear(_neighborStrengths, 0, _neighborStrengths.Length);

//        // Get and process nearby balls
//        var nearbyBalls = ball.BallFactory.GetActiveBalls()
//            .Where(b => b != null && b.gameObject != gameObject)
//            .OrderBy(b => Vector3.Distance(transform.position, b.transform.position))
//            .Take(maxNeighbors)
//            .ToList();

//        for (int i = 0; i < nearbyBalls.Count; i++)
//        {
//            Vector3 pos = nearbyBalls[i].transform.position;
//            float distance = Vector3.Distance(transform.position, pos);
//            float strength = 1 - Mathf.Clamp01(distance / proximityRadius);

//            _neighborPositions[i] = new Vector4(pos.x, pos.y, pos.z, 0);
//            _neighborStrengths[i] = strength * maxProximityStrength;
//        }
//    }

//    private void UpdateMaterialProperties()
//    {
//        _renderer.GetPropertyBlock(_props);

//        _props.SetVectorArray("_NeighborPositions", _neighborPositions);
//        _props.SetFloatArray("_NeighborStrengths", _neighborStrengths);
//        _props.SetInt("_NeighborCount", maxNeighbors);
//        _props.SetColor("_ProximityColor", proximityColor);

//        _renderer.SetPropertyBlock(_props);
//    }
//}