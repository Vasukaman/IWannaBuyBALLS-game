// Filename: Draggable2D.cs
using UnityEngine;

namespace Core.Input
{
    /// <summary>
    /// Allows a Rigidbody2D object to be dragged by the mouse.
    /// Includes an optional timeout to stop dragging if the mouse leaves the object.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Draggable2D : MonoBehaviour
    {
        [Header("Drag Settings")]
        [Tooltip("If true, the object's gravity will be set to 0 while being dragged.")]
        [SerializeField] private bool _disableGravityWhileDragging = true;

        [Tooltip("If greater than 0, dragging will stop after the mouse has been off the object for this many seconds.")]
        [SerializeField] private float _dragTimeout = 0.2f;

        // --- State & Cache ---
        private Rigidbody2D _rigidbody;
        private Camera _mainCamera;
        private Vector3 _mouseOffset;
        private float _originalGravityScale;
        private bool _isDragging = false;
        private bool _isMouseOver = false;
        private float _mouseOffTimer = 0f;

        // --- Unity Methods ---

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _originalGravityScale = _rigidbody.gravityScale;
            
            // TODO: [Dependency] Using Camera.main is convenient but creates a hard dependency. If the main camera
            // tag is missing, this will fail. A more robust solution would be to pass the camera reference
            // in through a manager or a public property.
            _mainCamera = Camera.main;
        }

        private void OnMouseDown()
        {
            if (_mainCamera == null) return;

            _isDragging = true;
            _mouseOffTimer = 0f; // Reset timeout timer
            
            // Calculate the offset from the object's center to the mouse click position
            Vector3 mouseWorldPosition = GetMouseWorldPosition();
            _mouseOffset = transform.position - mouseWorldPosition;

            if (_disableGravityWhileDragging)
            {
                _rigidbody.gravityScale = 0f;
            }
            _rigidbody.velocity = Vector2.zero;
        }

        private void OnMouseUp()
        {
            // This is called only when the mouse button is released *while over the object*.
            if (_isDragging)
            {
                StopDragging();
            }
        }
        
        private void OnMouseEnter()
        {
            _isMouseOver = true;
            _mouseOffTimer = 0f; // Reset timer when mouse re-enters
        }

        private void OnMouseExit()
        {
            _isMouseOver = false;
        }
        
        private void Update()
        {
            if (!_isDragging) return;

            // Handle drag timeout
            if (_dragTimeout > 0 && !_isMouseOver)
            {
                _mouseOffTimer += Time.deltaTime;
                if (_mouseOffTimer >= _dragTimeout)
                {
                    StopDragging();
                    return; // Stop further processing this frame
                }
            }
            
            // Continue dragging
            Vector3 targetPosition = GetMouseWorldPosition() + _mouseOffset;
            _rigidbody.MovePosition(targetPosition);
        }
        
        // --- Private Methods ---
        
        /// <summary>
        /// Stops the dragging process and restores the object's physics properties.
        /// </summary>
        private void StopDragging()
        {
            _isDragging = false;
            if (_disableGravityWhileDragging)
            {
                _rigidbody.gravityScale = _originalGravityScale;
            }
        }
        
        /// <summary>
        /// Calculates the current world position of the mouse cursor.
        /// </summary>
        private Vector3 GetMouseWorldPosition()
        {
            Vector3 mousePoint = UnityEngine.Input.mousePosition;
            // Ensure Z-coordinate is consistent for proper 2D depth calculation
            mousePoint.z = _mainCamera.WorldToScreenPoint(transform.position).z;
            return _mainCamera.ScreenToWorldPoint(mousePoint);
        }
    }
}