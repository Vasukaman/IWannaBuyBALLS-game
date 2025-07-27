using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages a single item in the store.
/// It handles instantiating a preview gadget, managing the drag-and-drop lifecycle,
/// and communicating with the StoreManager to handle purchase and refund logic
/// based on whether the item is dragged into the gameplay area or not.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class ShopItemController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private Image _iconImage;

    [Header("Object References")]
    [Tooltip("The parent object for all UI/Icon elements. This is hidden during drag.")]
    [SerializeField] private GameObject _iconRoot;

    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.15f;

    // --- Private State ---
    private GadgetData _data;
    private StoreManager _store;
    private Camera _mainCamera;
    private GameObject _gadgetInstance;

    private Vector3 _dragOffset;
    private bool _isDragging;
    private bool _isInGameplayArea;
    private Coroutine _activeAnimation;

    public void Initialize(GadgetData data, StoreManager store)
    {
        _data = data;
        _store = store;
        _mainCamera = Camera.main;

        if (_iconImage != null)
        {
            _iconImage.sprite = data.icon;
        }

        ResetState();
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        int currentPrice = _store.GetCurrentPrice(_data);
        bool canAfford = _store.CanAfford(_data);

        _priceText.text = $"{currentPrice}";
        _priceText.color = canAfford ? Color.green : Color.red;
        _iconImage.color = canAfford ? Color.white : new Color(1, 1, 1, 0.5f);
    }

    private void OnMouseDown()
    {
        if (!_store.CanAfford(_data) || _isDragging) return;

        _isDragging = true;
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        _dragOffset = transform.position - new Vector3(mouseWorldPos.x, mouseWorldPos.y, transform.position.z);
    }

    private void Update()
    {
        if (!_isDragging) return;

        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(
            mouseWorldPos.x + _dragOffset.x,
            mouseWorldPos.y + _dragOffset.y,
            transform.position.z
        );

        Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position);
        bool isCurrentlyInGameplayArea = screenPos.y > Screen.height * 0.25f;

        //_gadgetInstance.transform.localPosition = Vector3.zero;

        if (isCurrentlyInGameplayArea != _isInGameplayArea)
        {
            _isInGameplayArea = isCurrentlyInGameplayArea;

            // --- MODIFIED: The core logic now triggers one of two transition animations ---
            if (_activeAnimation != null)
            {
                StopCoroutine(_activeAnimation);
            }

            if (_isInGameplayArea)
            {
                // --- Entered Gameplay Area: Animate from Icon to Gadget ---
                _store.PurchaseItem(_data);
                _activeAnimation = StartCoroutine(AnimateTransitionToGadget());
            }
            else
            {
                // --- Returned to UI Area: Animate from Gadget to Icon ---
                _store.RefundPurchase(_data);
                _activeAnimation = StartCoroutine(AnimateTransitionToIcon());
            }
        }
    }

    private void OnMouseUp()
    {
        if (!_isDragging) return;
        _isDragging = false;

        if (_activeAnimation != null)
        {
            StopCoroutine(_activeAnimation);
            _activeAnimation = null;
        }

        if (_isInGameplayArea)
        {
            // --- Purchase Confirmed ---
            _gadgetInstance.transform.SetParent(null, true);
            _gadgetInstance.transform.localScale = Vector3.one;
        }
        else
        {
            // --- Purchase Cancelled ---
            Destroy(_gadgetInstance);
        }

        ResetState();
    }

    private void ResetState()
    {
        transform.localPosition = Vector3.zero;
        _iconRoot.SetActive(true);
        // --- MODIFIED: Ensure icon scale is correct on reset ---
        _iconRoot.transform.localScale = Vector3.one;
        _isInGameplayArea = false;

        if (_data != null && _data.prefab != null)
        {
            //TODO: this must be done with factory
            _gadgetInstance = Instantiate(_data.prefab, transform);
            _gadgetInstance.transform.localPosition = Vector3.zero;
            _gadgetInstance.SetActive(false);
            _gadgetInstance.GetComponent<Gadget>().storeManager = _store;
        }
    }

    // --- NEW: A coroutine to animate from the icon to the gadget ---
    private IEnumerator AnimateTransitionToGadget()
    {
        // 1. Shrink the icon
        float elapsedTime = 0f;
        while (elapsedTime < _animationDuration)
        {
            _gadgetInstance.transform.localPosition = Vector3.zero;
            _iconRoot.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsedTime / _animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _iconRoot.SetActive(false);

        // 2. Grow the gadget
        _gadgetInstance.SetActive(true);
        elapsedTime = 0f;
        while (elapsedTime < _animationDuration)
        {
            _gadgetInstance.transform.localPosition = Vector3.zero;
            _gadgetInstance.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsedTime / _animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _gadgetInstance.transform.localScale = Vector3.one; // Finalize scale
        _activeAnimation = null;
    }

    // --- NEW: A coroutine to animate from the gadget back to the icon ---
    private IEnumerator AnimateTransitionToIcon()
    {
        // 1. Shrink the gadget
        float elapsedTime = 0f;
        while (elapsedTime < _animationDuration)
        {
            _gadgetInstance.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsedTime / _animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _gadgetInstance.SetActive(false);

        // 2. Grow the icon
        _iconRoot.SetActive(true);
        elapsedTime = 0f;
        while (elapsedTime < _animationDuration)
        {
            _iconRoot.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsedTime / _animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _iconRoot.transform.localScale = Vector3.one; // Finalize scale
        _activeAnimation = null;
    }
}