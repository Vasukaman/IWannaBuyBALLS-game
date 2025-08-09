// Filename: StoreItemPresenter.cs
using Core.Input;
using Core.Interfaces;
using Gameplay.Gadgets;
using Gameplay.Placeables;
using Reflex.Attributes;
using Services.Gadgets;
using Services.Store;
using UnityEngine;
using Core.Data;
using Core.Events;


namespace UI.Store
{
    [RequireComponent(typeof(Draggable), typeof(StoreItemView))]
    public class StoreItemPresenter : MonoBehaviour
    {
        private enum State { Idle, Dragging }

        [Header("Configuration")]
        [Range(0, 1)][SerializeField] private float _gameplayAreaBoundary = 0.25f;
        [SerializeField] private float _animationDuration = 0.15f;

        [Inject] private IStoreService _storeService;
        [Inject] private IGadgetService _gadgetService;

        private Draggable _draggable;
        private StoreItemView _view;
        private Camera _mainCamera;

        private GadgetData _data;
        private PlaceableView _gadgetPreviewInstance;
        private State _currentState = State.Idle;
        private bool _isPreviewingGadget = false;

        [SerializeField] private GadgetGameEvent _onGadgetPlacedEvent;

        private void Awake()
        {
            _draggable = GetComponent<Draggable>();
            _view = GetComponent<StoreItemView>();
            _mainCamera = Camera.main;

            _draggable.OnDragStarted += HandleDragStarted;
            _draggable.OnDragUpdated += HandleDragUpdated;
            _draggable.OnDragEnded += HandleDragEnded;
        }

        public void Initialize(GadgetData data)
        {
            _data = data;
            _view.SetIcon(_data.Icon);
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_storeService == null || _data == null) return;
            _view.UpdateDisplay(_storeService.GetCurrentPrice(_data), _storeService.CanAfford(_data));
        }

        private void HandleDragStarted()
        {
            if (_currentState != State.Idle || !_storeService.CanAfford(_data)) return;

            _currentState = State.Dragging;

            // 1. Call the service, which returns the generic interface.
            IPlaceableView placeableInterface = _gadgetService.CreateGadget(_data, transform.position, transform);

            // 2. Safely cast the interface to the concrete class we know it is.
            _gadgetPreviewInstance = placeableInterface as PlaceableView;

            // 3. A crucial safety check.
            if (_gadgetPreviewInstance == null)
            {
                Debug.LogError("The created gadget is not a valid PlaceableView!", this);
                // We might need to handle the failed purchase here if it's possible.
                _currentState = State.Idle; // Reset state
                return;
            }

            _gadgetPreviewInstance.gameObject.SetActive(false);
        }

        private void HandleDragUpdated(Vector3 currentPosition)
        {
            if (_currentState != State.Dragging) return;

            bool isNowInGameplayArea = _mainCamera.WorldToScreenPoint(currentPosition).y > Screen.height * _gameplayAreaBoundary;

            if (isNowInGameplayArea != _isPreviewingGadget)
            {
                _isPreviewingGadget = isNowInGameplayArea;

                // The Presenter just gives a high-level command.
                if (_isPreviewingGadget)
                {
                    _view.AnimateToGadget(_gadgetPreviewInstance.gameObject, _animationDuration);
                }
                else
                {
                    _view.AnimateToIcon(_gadgetPreviewInstance.gameObject, _animationDuration);
                }
            }
        }

        private void HandleDragEnded()
        {
            if (_currentState != State.Dragging) return;

         //   _view.StopActiveAnimation();

            if (_isPreviewingGadget)
            {
                _storeService.PurchaseItem(_data);
                _onGadgetPlacedEvent.Raise(_data);
                _gadgetPreviewInstance.transform.SetParent(null, true);
            }
            else
            {
                Destroy(_gadgetPreviewInstance.gameObject);
            }

            _gadgetPreviewInstance = null;
            transform.localPosition = Vector3.zero;
            _view.ResetIcon();
            _view.AnimateIconAppearance(_animationDuration);
            _view.transform.localScale = Vector3.one;
            _currentState = State.Idle;
        }

        private void OnDestroy()
        {
            if (_draggable == null) return;
            _draggable.OnDragStarted -= HandleDragStarted;
            _draggable.OnDragUpdated -= HandleDragUpdated;
            _draggable.OnDragEnded -= HandleDragEnded;
        }
    }
}