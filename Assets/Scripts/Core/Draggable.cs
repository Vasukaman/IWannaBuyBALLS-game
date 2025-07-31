// Filename: Draggable.cs
using System;
using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// A generic component that allows a GameObject with a Collider2D to be dragged by the mouse.
    /// It intelligently handles both Rigidbody2D and non-Rigidbody objects.
    /// Fires events to notify other systems of drag state changes.
    /// </summary>
    public class Draggable : MonoBehaviour
    {
        #region Events
        public event Action OnDragStarted;
        public event Action<Vector3> OnDragUpdated; // The new event
        public event Action OnDragEnded;
        #endregion

        #region Configuration
        [Header("Physics Settings")]
        [Tooltip("If true, the object's gravity will be set to 0 while being dragged (requires a Rigidbody2D).")]
        [SerializeField] private bool _disableGravityWhileDragging = true;

        [Header("Timeout Settings")]
        [Tooltip("If greater than 0, dragging will stop after the mouse has been off the object for this many seconds.")]
        [SerializeField] private float _dragTimeout = 0.2f;
        [SerializeField] private bool _disableDragOnTimeout = true;
        #endregion

        #region State & Cache
        private Rigidbody2D _rigidbody;
        private Camera _mainCamera;
        private Vector3 _mouseOffset;
        private float _originalGravityScale;
        private bool _isDragging = false;
        private bool _isMouseOver = false;
        private float _mouseOffTimer = 0f;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            // Try to get a Rigidbody2D. If it doesn't exist, _rigidbody will be null.
            TryGetComponent<Rigidbody2D>(out _rigidbody);
            if (_rigidbody != null)
            {
                _originalGravityScale = _rigidbody.gravityScale;
            }

            _mainCamera = Camera.main;
        }

        private void OnMouseDown()
        {
            if (_mainCamera == null) return;

            _isDragging = true;
            _mouseOffTimer = 0f;

            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            _mouseOffset = transform.position - mouseWorldPosition;

            if (_rigidbody != null && _disableGravityWhileDragging)
            {
                _rigidbody.gravityScale = 0f;
            }

            OnDragStarted?.Invoke();
        }

        private void OnMouseUp()
        {
            if (_isDragging)
            {
                StopDragging();
            }
        }

        private void OnMouseEnter()
        {
            _isMouseOver = true;
            _mouseOffTimer = 0f;
        }

        private void OnMouseExit()
        {
            _isMouseOver = false;
        }

        private void FixedUpdate() // Use FixedUpdate for physics-based movement
        {
            if (!_isDragging) return;

            // Handle drag timeout logic in FixedUpdate to be in sync with physics
            if (HandleTimeout())
            {
                return; // Dragging was stopped by timeout
            }

            // Move the object
            Vector3 targetPosition = GetMouseWorldPosition() + _mouseOffset;

            // THE KEY LOGIC: Use the correct movement method
            if (_rigidbody != null)
            {
                _rigidbody.MovePosition(targetPosition);
            }
            else
            {
                transform.position = targetPosition;
            }
            OnDragUpdated?.Invoke(transform.position);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Stops the dragging process and restores the object's state.
        /// </summary>
        private void StopDragging()
        {
            _isDragging = false;
            if (_rigidbody != null && _disableGravityWhileDragging)
            {
                _rigidbody.gravityScale = _originalGravityScale;
            }
            OnDragEnded?.Invoke();
        }

        /// <summary>
        /// Checks and handles the drag timeout if the mouse is off the object.
        /// </summary>
        /// <returns>True if the drag was stopped, false otherwise.</returns>
        private bool HandleTimeout()
        {if (!_disableDragOnTimeout) return false;
            if (_dragTimeout > 0 && !_isMouseOver)
            {
                _mouseOffTimer += Time.fixedDeltaTime;
                if (_mouseOffTimer >= _dragTimeout)
                {
                    StopDragging();
                    return true;
                }
            }
            return false;
        }

        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePoint = UnityEngine.Input.mousePosition;
            mousePoint.z = _mainCamera.WorldToScreenPoint(transform.position).z;
            return _mainCamera.ScreenToWorldPoint(mousePoint);
        }
        #endregion
    }
}