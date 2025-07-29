// Filename: ShopItemController.cs
using Gameplay.Gadgets;
using System.Collections;
using TMPro;
using UI.Store;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Store
{
    // TODO: [SRP Violation] This class is complex. It acts as a View (UpdateDisplay), a Controller (handling drag input),
    // and a Presenter (coordinating with the StoreManager and running animations). For a larger project, this could be
    // split into smaller classes (e.g., a separate DragHandler, a separate Animator). For this scope, it is manageable.
    [RequireComponent(typeof(BoxCollider2D))]
    public class StoreSlotController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _priceText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _iconRoot;

        [Header("Drag & Drop Settings")]
        [Tooltip("The normalized screen height (0-1) that defines the boundary of the gameplay area.")]
        [Range(0, 1)]
        [SerializeField] private float _gameplayAreaBoundary = 0.25f;

        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.15f;

        // --- State & Cache ---
        private GadgetData _data;
        private StoreManager _store;
        private Camera _mainCamera;
        private GameObject _gadgetInstance;
        private Vector3 _dragOffset;
        private bool _isDragging;
        private bool _isInGameplayArea;
        private Coroutine _activeAnimation;

        // --- Public API ---

        /// <summary>
        /// Initializes the shop item with its data and a reference to the store manager.
        /// </summary>
        public void Initialize(GadgetData data, StoreManager store)
        {
            _data = data;
            _store = store;
            _mainCamera = Camera.main;

            if (_iconImage != null)
            {
                _iconImage.sprite = data.Icon;
            }

            UpdateDisplay();
            ResetAndPreparePreview();
        }

        /// <summary>
        /// Updates the price and visual state based on whether the player can afford the item.
        /// </summary>
        public void UpdateDisplay()
        {
            if (_store == null || _data == null) return;

            int currentPrice = _store.GetCurrentPrice(_data);
            bool canAfford = _store.CanAfford(_data);

            _priceText.text = $"{currentPrice}";
            _priceText.color = canAfford ? Color.green : Color.red;
            _iconImage.color = canAfford ? Color.white : new Color(1, 1, 1, 0.5f);
        }

        // --- Unity Input Methods ---

        private void OnMouseDown()
        {
            if (!_store.CanAfford(_data) || _isDragging) return;

            _isDragging = true;
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            _dragOffset = transform.position - mouseWorldPos;
        }

        private void Update()
        {
            if (!_isDragging) return;

            HandleDragMovement();
            CheckForAreaTransition();
        }

        private void OnMouseUp()
        {
            if (!_isDragging) return;

            _isDragging = false;
            StopActiveAnimation();

            if (_isInGameplayArea)
            {
                ConfirmPurchase();
            }
            else
            {
                CancelPurchase();
            }

            ResetAndPreparePreview();
        }

        // --- Drag & Drop Logic ---

        private void HandleDragMovement()
        {
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            transform.position = new Vector3(
                mouseWorldPos.x + _dragOffset.x,
                mouseWorldPos.y + _dragOffset.y,
                transform.position.z
            );
        }

        private void CheckForAreaTransition()
        {
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position);
            bool isNowInGameplayArea = screenPos.y > Screen.height * _gameplayAreaBoundary;

            if (isNowInGameplayArea != _isInGameplayArea)
            {
                _isInGameplayArea = isNowInGameplayArea;
                StopActiveAnimation();

                if (_isInGameplayArea)
                {
                    _store.PurchaseItem(_data);
                    _activeAnimation = StartCoroutine(AnimateTransition(from: _iconRoot, to: _gadgetInstance));
                }
                else
                {
                    _store.RefundPurchase(_data);
                    _activeAnimation = StartCoroutine(AnimateTransition(from: _gadgetInstance, to: _iconRoot));
                }
            }
        }

        private void ConfirmPurchase()
        {
            _gadgetInstance.transform.SetParent(null, true);
            _gadgetInstance.transform.localScale = Vector3.one;
        }

        private void CancelPurchase()
        {
            Destroy(_gadgetInstance);
        }

        /// <summary>
        /// Resets the item's position and prepares a new, inactive gadget preview for the next drag.
        /// </summary>
        private void ResetAndPreparePreview()
        {
            transform.localPosition = Vector3.zero;
            _iconRoot.SetActive(true);
            _iconRoot.transform.localScale = Vector3.one;
            _isInGameplayArea = false;

            if (_data?.Prefab != null)
            {
                // TODO: [Decoupling] This UI component should not be responsible for instantiating game objects.
                // This logic should be delegated to a GadgetFactory to keep UI separate from game object creation.
                _gadgetInstance = Instantiate(_data.Prefab, transform);
                _gadgetInstance.transform.localPosition = Vector3.zero;
                _gadgetInstance.SetActive(false);


                // TODO: [Manual Dependency Injection] This is a "code smell". The shop item has to know about the
                // internal dependencies of the Gadget it creates. A proper DI framework or service locator
                // should handle providing the StoreManager to the Gadget instance automatically.
                if (_gadgetInstance.TryGetComponent<Gadget>(out var gadget))
                {
                    gadget._storeManager = _store; // //TODO: VERY BAD!!!! Should be easily fiable with proper injector.
                    gadget.Initialize(_data);
                }
            }
        }

        // --- Animation ---

        private void StopActiveAnimation()
        {
            if (_activeAnimation != null)
            {
                StopCoroutine(_activeAnimation);
                _activeAnimation = null;
            }
        }

        /// <summary>
        /// A generic coroutine to animate a fade-out/fade-in transition between two GameObjects by scaling them.
        /// </summary>
        private IEnumerator AnimateTransition(GameObject from, GameObject to)
        {
            // 1. Shrink the 'from' object
            yield return AnimateScale(from.transform, Vector3.one, Vector3.zero);
            from.SetActive(false);

            // 2. Grow the 'to' object
            to.SetActive(true);
            yield return AnimateScale(to.transform, Vector3.zero, Vector3.one);

            _activeAnimation = null;
        }

        private IEnumerator AnimateScale(Transform targetTransform, Vector3 startScale, Vector3 endScale)
        {
            float elapsedTime = 0f;
            while (elapsedTime < _animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / _animationDuration);
                targetTransform.localScale = Vector3.Lerp(startScale, endScale, progress);
                yield return null;
            }
            targetTransform.localScale = endScale;
        }

        // --- Helpers ---

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePoint = Input.mousePosition;
            mousePoint.z = _mainCamera.WorldToScreenPoint(transform.position).z;
            return _mainCamera.ScreenToWorldPoint(mousePoint);
        }
    }
}