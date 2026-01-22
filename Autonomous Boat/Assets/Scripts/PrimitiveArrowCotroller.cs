using UnityEngine;

public class PrimitiveArrowController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotateSpeed = 90f;

    void Update()
    {
        // Move forward/backward in boat's facing direction (XZ plane)
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Vector3 dir = transform.forward;
            dir.y = 0f;                 // lock to XZ plane
            transform.position += dir.normalized * moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            Vector3 dir = transform.forward;
            dir.y = 0f;
            transform.position -= dir.normalized * moveSpeed * Time.deltaTime;
        }

        // Rotate around Y axis
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime, Space.World);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        }
    }
}