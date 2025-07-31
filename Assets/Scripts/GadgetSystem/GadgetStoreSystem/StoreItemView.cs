// Filename: StoreItemView.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Store
{
    public class StoreItemView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _iconRoot; // This remains private to the View

        [Header("Display Settings")]
        [SerializeField] private Color _canAffordColor = Color.green;
        [SerializeField] private Color _cannotAffordColor = Color.red;
        [SerializeField] private float _cannotAffordAlpha = 0.5f;

        private Coroutine _activeAnimation;

        public void SetIcon(Sprite icon) => _iconImage.sprite = icon;

        public void UpdateDisplay(int price, bool canAfford)
        {
            _priceText.text = price.ToString();
            _priceText.color = canAfford ? _canAffordColor : _cannotAffordColor;
            _iconImage.color = canAfford ? Color.white : new Color(1, 1, 1, _cannotAffordAlpha);
        }

        private void SetIconVisibility(bool isVisible) => _iconRoot.SetActive(isVisible);
        private void ReseticonScale() => _iconRoot.transform.localScale = Vector3.one;

        public void ResetIcon()
        { SetIconVisibility(true);
          ReseticonScale();
        }


        // --- NEW: Public methods to control animations ---

        public void AnimateToGadget(GameObject gadgetInstance, float duration)
        {
            StopActiveAnimation();
            _activeAnimation = StartCoroutine(AnimateTransitionCoroutine(_iconRoot, gadgetInstance, duration));
        }

        public void AnimateToIcon(GameObject gadgetInstance, float duration)
        {
            StopActiveAnimation();
            _activeAnimation = StartCoroutine(AnimateTransitionCoroutine(gadgetInstance, _iconRoot, duration));
        }

        public void StopActiveAnimation()
        {
            if (_activeAnimation != null)
            {
                StopCoroutine(_activeAnimation);
                _activeAnimation = null;
            }
        }

        // --- Private Animation Logic ---

        private IEnumerator AnimateTransitionCoroutine(GameObject from, GameObject to, float duration)
        {
            // 1. Shrink the 'from' object
            yield return AnimateScale(from.transform, Vector3.one, Vector3.zero, duration);
            from.SetActive(false);

            // 2. Grow the 'to' object
            to.SetActive(true);
            yield return AnimateScale(to.transform, Vector3.zero, Vector3.one, duration);

            _activeAnimation = null;
        }

        private IEnumerator AnimateScale(Transform target, Vector3 start, Vector3 end, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / duration);
                target.localScale = Vector3.Lerp(start, end, progress);
                yield return null;
            }
            target.localScale = end;
        }
    }
}