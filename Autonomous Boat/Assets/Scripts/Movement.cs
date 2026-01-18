using UnityEngine;

public class BoatDifferentialArrows : MonoBehaviour
{
    public Rigidbody Boat;

    [Header("Motors")]
    public Transform leftMotor;   // left side of boat
    public Transform rightMotor;  // right side of boat

    [Header("Thrust Settings")]
    public float maxThrustPerMotor = 50f;   // N
    public float turnFactor = 0.6f;         // 0..1 how strong turning is while moving

    [Header("Stabilization")]
    public float yawDamping = 8f;           // higher = less unwanted spinning
    public float inputDeadzone = 0.01f;

    void Reset()
    {
        Boat = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Boat == null) return;

        // --- 1) Read arrow keys ---
        int forwardDir = 0; // -1 backward, 0 none, +1 forward
        if (Input.GetKey(KeyCode.UpArrow)) forwardDir += 1;
        if (Input.GetKey(KeyCode.DownArrow)) forwardDir -= 1;

        int turnDir = 0; // -1 left, 0 none, +1 right
        if (Input.GetKey(KeyCode.RightArrow)) turnDir += 1;
        if (Input.GetKey(KeyCode.LeftArrow)) turnDir -= 1;

        // --- 2) Convert to motor powers (-1..1) ---
        float leftPower = 0f;
        float rightPower = 0f;

        if (forwardDir != 0)
        {
            // Moving: differential thrust for steering
            // IMPORTANT: when reversing, steering direction flips
            float steer = turnDir * turnFactor;
            if (forwardDir < 0) steer = -steer;

            leftPower = Mathf.Clamp(forwardDir + steer, -1f, 1f);
            rightPower = Mathf.Clamp(forwardDir - steer, -1f, 1f);
        }
        else if (turnDir != 0)
        {
            // In-place turn: opposite thrust (NOT torque)
            leftPower = Mathf.Clamp(-turnDir, -1f, 1f);
            rightPower = Mathf.Clamp(+turnDir, -1f, 1f);
        }
        else
        {
            // No input: damp unwanted yaw (stop spinning)
            Vector3 ang = Boat.angularVelocity;
            Boat.angularVelocity = new Vector3(ang.x, ang.y * Mathf.Clamp01(1f - yawDamping * Time.fixedDeltaTime), ang.z);
            return;
        }

        // deadzone
        if (Mathf.Abs(leftPower) < inputDeadzone) leftPower = 0f;
        if (Mathf.Abs(rightPower) < inputDeadzone) rightPower = 0f;

        // --- 3) Apply forces at motor positions ---
        Vector3 fwd = Boat.transform.forward;

        if (leftMotor != null && leftPower != 0f)
        {
            Vector3 leftForce = fwd * (leftPower * maxThrustPerMotor);
            Boat.AddForceAtPosition(leftForce, leftMotor.position, ForceMode.Force);
        }

        if (rightMotor != null && rightPower != 0f)
        {
            Vector3 rightForce = fwd * (rightPower * maxThrustPerMotor);
            Boat.AddForceAtPosition(rightForce, rightMotor.position, ForceMode.Force);
        }

        // Extra stabilization: damp yaw a bit even while moving (prevents long drift spin)
        {
            Vector3 ang = Boat.angularVelocity;
            Boat.angularVelocity = new Vector3(ang.x, ang.y * Mathf.Clamp01(1f - (yawDamping * 0.5f) * Time.fixedDeltaTime), ang.z);
        }
    }
}
