using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Draggable2D : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool dragging = false;
    private Vector3 offset;
    private Camera cam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    void OnMouseDown()
    {
        dragging = true;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z);
     //   rb.gravityScale = 0f; // optional: turn off gravity while dragging
        rb.velocity = Vector2.zero;
    }

    void OnMouseUp()
    {
        dragging = false;
      //  rb.gravityScale = 1f; // restore if using gravity
    }

    void Update()
    {
        if (dragging)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 targetPos = new Vector3(mouseWorld.x, mouseWorld.y, transform.position.z) + offset;
            rb.MovePosition(targetPos); // physics-friendly movement
        }
    }
}
