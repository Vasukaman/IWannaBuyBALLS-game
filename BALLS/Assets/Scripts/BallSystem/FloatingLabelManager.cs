using TMPro;
using UnityEngine;

public class FloatingLabelManager : MonoBehaviour
{
    [Tooltip("The ball (or any world-space object) whose position we follow.")]
    [SerializeField] Transform target;

    [Tooltip("Optional world-space offset above the target.")]
    [SerializeField] Vector3 worldOffset = Vector3.up * 0.6f;
    [SerializeField] TextMeshProUGUI tmp;
    [SerializeField] float baseScaleMult;
    [SerializeField] float maxScale = 0.8f;
    Transform cachedTransform;

    void Awake()
    {
        cachedTransform = transform;

        // 1) Un-parent
        cachedTransform.SetParent(null, worldPositionStays: true);

        // 2) Clamp scale to 1,1,1
        cachedTransform.localScale = Vector3.one;
    }


    void LateUpdate()
    {
        bool alive = target != null && target.gameObject.activeInHierarchy;

        tmp.gameObject.SetActive(alive);   // TMP on/off
        if (alive)
        {
            tmp.transform.position = target.position + worldOffset;
            tmp.transform.localScale = Vector3.one * Mathf.Min(maxScale, target.lossyScale.x*baseScaleMult);
        }
    }
}