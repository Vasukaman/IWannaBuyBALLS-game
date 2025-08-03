// Filename: GadgetSellZonePresenter.cs
using Core.Animation;
using Gameplay.Gadgets;
using System.Collections;
using UnityEngine;
using VFX.Gadgets;

namespace Gameplay.Gadgets
{
    [RequireComponent(typeof(GadgetSellTrigger), typeof(SpringTransform), typeof(SellZoneView))]
    public class GadgetSellZonePresenter : MonoBehaviour
    {

        [Header("Configuration")]
        [Tooltip("The master profile that defines all behavior for this sell zone.")]
        [SerializeField] private SellZoneProfile _profile;

        // Public property for children to access the profile.
        public SellZoneProfile Profile => _profile;


        [Header("Configuration")]
        //[SerializeField] private float _timeToSell = 3.0f;
        //[SerializeField] private float _maxTrackingDistance = 3.0f; 
        //[SerializeField] private float _capturePadding = 0.5f;

        private GadgetSellTrigger _trigger;
        private SpringTransform _spring;
        private SellZoneView _view;
        private BoxCollider2D _collider;
        private IGadgetSellable _currentTarget;
        private Coroutine _sellingCoroutine;
        private Vector3 _originalPosition;
        private Vector3 _originalScale;

        private void Awake()
        {
            if (_profile == null)
            {
                Debug.LogError("GadgetSellZonePresenter is missing a Profile! Disabling component.", this);
                enabled = false;
                return;
            }


            _trigger = GetComponent<GadgetSellTrigger>();
            _spring = GetComponent<SpringTransform>();
            _view = GetComponent<SellZoneView>();
            _collider = GetComponent<BoxCollider2D>();
            _originalPosition = transform.position;
            _originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            _trigger.OnGadgetEntered += HandleGadgetEntered;
            _trigger.OnGadgetExited += HandleGadgetExited;
        }

        private void OnDisable()
        {
            _trigger.OnGadgetEntered -= HandleGadgetEntered;
            _trigger.OnGadgetExited -= HandleGadgetExited;
        }

        private void Update()
        {
            if (_currentTarget != null)
            {
                UpdateSpringTargetToTrackGadget();
            }
            else
            {
                ResetSpringTargetToOriginal();
            }
        }

        private void HandleGadgetEntered(IGadgetSellable sellable)
        {
            if (_currentTarget != null) return;
            _currentTarget = sellable;
            _sellingCoroutine = StartCoroutine(SellProcessCoroutine());
        }

        private void HandleGadgetExited(IGadgetSellable sellable)
        {
            if (_currentTarget == sellable)
            {
                StopAllProcesses();
            }
        }

        private IEnumerator SellProcessCoroutine()
        {
            yield return new WaitUntil(() => _currentTarget != null && IsFullyContained(_currentTarget.ObjectCollider));
            if (_currentTarget == null) yield break;

            float elapsedTime = 0f;
            while (elapsedTime < _profile.TimeToSell)
            {
                if (_currentTarget == null) { StopAllProcesses(); yield break; }

                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / _profile.TimeToSell;

                // --- FIX: Read acceleration from the View ---
                float accelerationFactor = 1.0f + (elapsedTime * _view.SpeedAcceleration);

                _view.UpdateSellingAppearance(progress, accelerationFactor);
                yield return null;
            }

            if (_currentTarget != null)
            {
                _currentTarget.Sell();
            }
            StopAllProcesses();
        }

        private void StopAllProcesses()
        {
            if (_sellingCoroutine != null)
            {
                StopCoroutine(_sellingCoroutine);
                _sellingCoroutine = null;
            }
            _currentTarget = null;
            _view.SetDefaultAppearance();
        }

        private void UpdateSpringTargetToTrackGadget()
        {
            Vector3 anchorCorner = new Vector3(_originalPosition.x + _originalScale.x / 2, _originalPosition.y + _originalScale.y / 2, 0);

            // --- FIX: Use ClosestPoint for accurate distance checking ---
            Vector3 closestPointOnGadget = _currentTarget.ObjectCollider.bounds.ClosestPoint(anchorCorner);
            if (Vector3.Distance(anchorCorner, closestPointOnGadget) > _profile.MaxTrackingDistance)
            {
                StopAllProcesses();
                return;
            }

            Vector3 targetCorner = _currentTarget.ObjectCollider.bounds.min - new Vector3(_profile.CapturePadding, _profile.CapturePadding, 0);
            Vector3 newScale = new Vector3(anchorCorner.x - targetCorner.x, anchorCorner.y - targetCorner.y, _originalScale.z);
            Vector3 newPosition = new Vector3(targetCorner.x + newScale.x / 2.0f, targetCorner.y + newScale.y / 2.0f, _originalPosition.z);

            _spring.TargetPosition = newPosition;
            _spring.TargetScale = newScale;
        }

        private void ResetSpringTargetToOriginal()
        {
            _spring.TargetPosition = _originalPosition;
            _spring.TargetScale = _originalScale;
        }

        private bool IsFullyContained(Collider2D other)
        {
            return true;
        }
    }
}