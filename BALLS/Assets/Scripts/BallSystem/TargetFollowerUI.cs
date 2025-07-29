// Filename: TargetFollowerUI.cs
using TMPro;
using UnityEngine;

namespace UI.WorldSpace
{
    /// <summary>
    /// Manages a UI element, making it follow a world-space Transform.
    /// It detaches itself from its parent on startup to maintain a consistent scale.
    /// </summary>
    public class TargetFollowerUI : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("The world-space object this UI element should follow.")]
        [SerializeField] private Transform _target;

        [Tooltip("An optional world-space offset from the target's position.")]
        [SerializeField] private Vector3 _worldOffset = Vector3.up * 0.6f;

        [Tooltip("The multiplier applied to the target's scale to determine the label's scale.")]
        [SerializeField] private float _baseScaleMultiplier = 0.5f;

        [Tooltip("The absolute maximum scale this UI element can have.")]
        [SerializeField] private float _maxScale = 0.8f;

        [Header("References")]
        [Tooltip("The TextMeshPro UI component to manage.")]
        [SerializeField] private TextMeshProUGUI _label;

        private Transform _thisTransform;

        // --- Unity Methods ---

        private void Awake()
        {
            _thisTransform = transform;

            // TODO: [Architecture] This component modifies its own place in the scene hierarchy at runtime.
            // This pattern works but can be fragile. It assumes this object starts as a child of a
            // scaled object and needs to be "liberated" to the scene root. A more robust architecture
            // might involve a central UI manager that instantiates these followers directly onto a world-space canvas.
            if (_thisTransform.parent != null)
            {
                _thisTransform.SetParent(null, worldPositionStays: true);
            }

            // Ensure the follower's container itself has a neutral scale.
            _thisTransform.localScale = Vector3.one;
        }

        private void LateUpdate()
        {
            if (_target == null || _label == null)
            {
                // If the target or label is missing, ensure the label is hidden.
                if (_label != null && _label.gameObject.activeSelf)
                {
                    _label.gameObject.SetActive(false);
                }
                return;
            }

            // Update visibility and position based on the target's state.
            bool isTargetActive = _target.gameObject.activeInHierarchy;
            UpdateVisibility(isTargetActive);

            if (isTargetActive)
            {
                UpdatePositionAndScale();
            }
        }

        // --- Private Methods ---

        /// <summary>
        /// Toggles the visibility of the label based on the target's active state.
        /// </summary>
        private void UpdateVisibility(bool isTargetActive)
        {
            if (_label.gameObject.activeSelf != isTargetActive)
            {
                _label.gameObject.SetActive(isTargetActive);
            }
        }

        /// <summary>
        /// Updates the position and scale of the label to match the target.
        /// </summary>
        private void UpdatePositionAndScale()
        {
            // Update position to follow the target with an offset.
            _label.transform.position = _target.position + _worldOffset;

            // Update scale based on the target's scale, clamped to a max value.
            float targetDrivenScale = _target.lossyScale.x * _baseScaleMultiplier;
            float finalScale = Mathf.Min(_maxScale, targetDrivenScale);
            _label.transform.localScale = Vector3.one * finalScale;
        }
    }
}