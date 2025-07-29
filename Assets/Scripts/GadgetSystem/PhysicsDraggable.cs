using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsDraggable : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Camera _cam;
    private Vector3 _offset;
    private bool _isDragging;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _cam = Camera.main;
    }

    private void OnMouseDown()
    {
        _isDragging = true;
        _rb.gravityScale = 0;
        _offset = transform.position - GetMouseWorldPos();
    }

    private void OnMouseUp()
    {
        _isDragging = false;
        _rb.gravityScale = 1;
    }

    private void FixedUpdate()
    {
        if (!_isDragging) return;

        Vector3 targetPos = GetMouseWorldPos() + _offset;
        Vector2 moveDirection = (targetPos - transform.position);
        _rb.velocity = moveDirection * 15f;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = -_cam.transform.position.z;
        return _cam.ScreenToWorldPoint(mouse);
    }
}