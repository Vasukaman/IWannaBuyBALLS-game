using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class ShopItemController : MonoBehaviour
{
    [SerializeField] private TMP_Text _priceText;

    private GadgetData _data;
    private StoreManager _store;
    private Camera _mainCamera;
    private Vector3 _dragOffset;
    private bool _isDragging;
   // private Vector3 _originalPosition;
    private Transform _originalParent;
    private int _currentPrice;

    public void Initialize(GadgetData data, StoreManager store)
    {
        _data = data;
        _store = store;
        _mainCamera = Camera.main;

        GetComponentInChildren<Image>().sprite = data.icon;
        UpdateDisplay();

    //    _originalParent = transform.parent;
        //_originalPosition = transform.position;
    }

    public void UpdateDisplay()
    {
        _currentPrice = _store.GetCurrentPrice(_data);
        bool canAfford = _store.CanAfford(_data);

        _priceText.text = $"{_currentPrice}";
        _priceText.color = canAfford ? Color.green : Color.red;
        GetComponent<SpriteRenderer>().color = canAfford ? Color.white : new Color(1, 1, 1, 0.5f);
    }

    private void OnMouseDown()
    {
        if (!_store.CanAfford(_data)) return;

        _isDragging = true;
        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        _dragOffset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z);
    }

    private void OnMouseUp()
    {
        if (!_isDragging) return;

        _isDragging = false;

        // Check if in gameplay area (top 80% of screen)
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(transform.position);
        if (screenPos.y > Screen.height * 0.25f)
        {
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            _store.PlaceGadget(_data, worldPos);
        }

        // Return to original position
      //  transform.SetParent(_originalParent);
        transform.localPosition = Vector3.zero;
    }

    private void Update()
    {
        if (!_isDragging) return;

        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(
            mouseWorld.x + _dragOffset.x,
            mouseWorld.y + _dragOffset.y,
            transform.position.z
        );
    }
}