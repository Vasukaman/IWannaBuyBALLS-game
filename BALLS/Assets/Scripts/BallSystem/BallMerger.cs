// BallMerger.cs - Handles merging logic
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Ball))]
public class BallMerger : MonoBehaviour
{
    [Header("Merge Settings")]
    [SerializeField] private bool allowMerge = true;
    [SerializeField] private float mergeDuration = 0.35f;
    [SerializeField] private float mergeCooldownAfterSpawn = 0.5f;
    [SerializeField] private float visualRadiusMultiplier = 0.8f;
    [SerializeField] private float positionCorrectionFactor = 1.2f;
    [SerializeField] private float maxVelocityToMerge = 5f;

    [Header("Merge Visualization")]
    [SerializeField] private Renderer ballRenderer;

    private Ball _ball;
    private Rigidbody2D _rb;
    private MaterialPropertyBlock _mpb;
    private bool _canMerge = false;

    private static readonly int MergeTargetPosID = Shader.PropertyToID("_MergeTargetPos");
    private static readonly int MergeTargetRadiusID = Shader.PropertyToID("_MergeTargetRadius");
    private static readonly int MergeWeightID = Shader.PropertyToID("_MergeWeight");

    private void Awake()
    {
        _ball = GetComponent<Ball>();
        _rb = GetComponent<Rigidbody2D>();
        _mpb = new MaterialPropertyBlock();
        ballRenderer.GetPropertyBlock(_mpb);
        _ball.OnInitialize += Initialize;
    }

    private void Start()
    {
      
    }

    public void Initialize(Ball _ball)
    {
        _canMerge = false;
        StartCoroutine(EnableMergeAfterDelay());
    }
    

    private IEnumerator EnableMergeAfterDelay()
    {
        yield return new WaitForSeconds(mergeCooldownAfterSpawn);
        _canMerge = true;
    }

    public void TryMerge(Ball other)
    {
        if (!allowMerge || !_canMerge) return;
        if (_ball.CurrentPrice != other.CurrentPrice) return;
        if (GetInstanceID() > other.GetInstanceID()) return;

        if (_rb.velocity.magnitude > maxVelocityToMerge) return;
        if (other.GetComponent<Rigidbody2D>().velocity.magnitude > maxVelocityToMerge) return;//TODO;This also shouldn't be like this;


        BallMerger otherMerger = other.GetComponent<BallMerger>(); //TODO: THis is bad, fix it.
        if (!otherMerger._canMerge) return;

        otherMerger._canMerge = false;
        _canMerge = false;
        StartCoroutine(MergeCoroutine(other));
    }

    private IEnumerator MergeCoroutine(Ball other)
    {
        //_rb.simulated = false;
        //  other.GetComponent<Rigidbody2D>().simulated = false;
        other.gameObject.layer = 6;
        float t = 0;
        Vector3 startPosition = other.transform.position;
        Vector3 survivorPosition = transform.position;
        Vector3 mergeCenter = (survivorPosition + startPosition) * 0.5f;

        BallMerger otherMerger = other.GetComponent<BallMerger>();
        float survivorWorldRadius = _ball.Radius * visualRadiusMultiplier;
        float otherWorldRadius = other.Radius * visualRadiusMultiplier;

        while (t < mergeDuration)
        {
            t += Time.deltaTime;
            float weight = Mathf.SmoothStep(0, 1, t / mergeDuration);

            other.transform.position = Vector3.Lerp(startPosition, mergeCenter, weight);
            transform.position = Vector3.Lerp(survivorPosition, mergeCenter, weight);

            UpdateMergeShaderParams(other, 1, otherWorldRadius);
            otherMerger.UpdateMergeShaderParams(_ball, 1, survivorWorldRadius);

            yield return null;
        }

        _ball.MultiplyPrice(2f);
        other.gameObject.layer = 3;
        other.Despawn();
        _canMerge = true;
        otherMerger._canMerge = true; // TODO; THis probably shoudn't be here, I think.
        //  _rb.simulated = true;
        ResetMergeShaderParams();
        otherMerger.ResetMergeShaderParams();
    }

    public void UpdateMergeShaderParams(Ball other, float weight, float otherRadius)
    {
        Vector3 otherLocalPos = transform.InverseTransformPoint(other.transform.position);
        Vector2 otherUV = new Vector2(
            0.5f + (otherLocalPos.x * positionCorrectionFactor) / transform.lossyScale.x,
            0.5f + (otherLocalPos.y * positionCorrectionFactor) / transform.lossyScale.y
        );

        float maxScale = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
        float uvRadius = otherRadius / maxScale;

        _mpb.SetVector(MergeTargetPosID, new Vector4(otherUV.x, otherUV.y, 0, 0));
        _mpb.SetFloat(MergeTargetRadiusID, uvRadius);
        _mpb.SetFloat(MergeWeightID, weight);
        ballRenderer.SetPropertyBlock(_mpb);
    }

    public void ResetMergeShaderParams()
    {
        _mpb.SetVector(MergeTargetPosID, Vector4.zero);
        _mpb.SetFloat(MergeTargetRadiusID, 0);
        _mpb.SetFloat(MergeWeightID, 0);
        ballRenderer.SetPropertyBlock(_mpb);
    }
}