using UnityEngine;

public class ArrowKeyBoatController : MonoBehaviour
{
    public Rigidbody rb;

    [Header("Movement speeds")]
    public float linearSpeed = 3f;    // units per second
    public float angularSpeed = 1.5f; // rad/s

    float linX;
    float angZ;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("ArrowKeyBoatController: Rigidbody not found.");
            enabled = false;
        }
    }

    void FixedUpdate()
    {
        // --- Read arrow keys ---
        linX = 0f;
        angZ = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
            linX = linearSpeed;

        if (Input.GetKey(KeyCode.DownArrow))
            linX = -linearSpeed;

        if (Input.GetKey(KeyCode.LeftArrow))
            angZ = -angularSpeed;

        if (Input.GetKey(KeyCode.RightArrow))
            angZ = angularSpeed;

        // --- Apply velocities ---
        rb.linearVelocity = transform.forward * linX;
        rb.angularVelocity = Vector3.up * angZ;
    }
}
