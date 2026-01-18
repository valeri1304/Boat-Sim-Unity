using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;

public class CmdVelPublisher : MonoBehaviour
{
    public string topicName = "/cmd_vel";
    private ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistMsg>(topicName);
        Debug.Log("Registered publisher: " + topicName);
    }

    void Update()
    {
        var msg = new TwistMsg();
        msg.linear.x = 0.5;
        msg.angular.z = 0.2;
        ros.Publish(topicName, msg);
    }
}
