using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class CmdVelBoatController : MonoBehaviour
{
    public string topic = "/cmd_vel";
    public Rigidbody rb;

    public float linearScale = 1f;   // m/s -> Unity units/s
    public float angularScale = 1f;  // rad/s

    float linX, angZ;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("CmdVelBoatController: No Rigidbody found. Add one or assign rb.");
            enabled = false;
            return;
        }

        ROSConnection.GetOrCreateInstance().Subscribe<TwistMsg>(topic, msg =>
        {
            linX = (float)msg.linear.x * linearScale;
            angZ = (float)msg.angular.z * angularScale;
        });
    }

    void FixedUpdate()
    {
        rb.linearVelocity = transform.forward * linX;
        rb.angularVelocity = Vector3.up * angZ;
    }

}
