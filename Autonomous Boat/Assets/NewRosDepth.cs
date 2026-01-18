using System;
using System.Linq;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class RosDepth32FC1Publisher : MonoBehaviour
{
    public Camera depthCam;
    public string topic = "/camera/depth/image_raw";
    public string frameId = "camera_depth_frame";

    public int width = 1280;
    public int height = 720;
    public int fps = 30;

    ROSConnection ros;
    Texture2D depthCpu; // reads RFloat RT
    float nextTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topic);

        if (depthCam == null)
            throw new Exception("RosDepth32FC1Publisher: Assign depthCam in Inspector.");

        if (depthCam.targetTexture == null)
            throw new Exception("RosDepth32FC1Publisher: Depth camera needs a Target Texture (RenderTexture).");

        depthCpu = new Texture2D(width, height, TextureFormat.RFloat, false, true);
    }

    void Update()
    {
        if (Time.unscaledTime < nextTime) return;
        nextTime = Time.unscaledTime + 1f / Mathf.Max(1, fps);

        RenderTexture rt = depthCam.targetTexture;

        RenderTexture.active = rt;
        depthCpu.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        depthCpu.Apply(false);
        RenderTexture.active = null;

        var meters = depthCpu.GetRawTextureData<float>();

        // float[] -> byte[]
        byte[] bytes = new byte[meters.Length * sizeof(float)];
        Buffer.BlockCopy(meters.ToArray(), 0, bytes, 0, bytes.Length);

        // time stamp
        double t = Time.realtimeSinceStartupAsDouble;
        int sec = (int)t;
        uint nanosec = (uint)((t - sec) * 1e9);

        var stamp = new TimeMsg();
        stamp.sec = sec;
        stamp.nanosec = nanosec;

        var header = new HeaderMsg();
        header.stamp = stamp;
        header.frame_id = frameId;

        var msg = new ImageMsg();
        msg.header = header;
        msg.height = (uint)height;
        msg.width = (uint)width;
        msg.encoding = "32FC1";
        msg.is_bigendian = 0;
        msg.step = (uint)(width * 4);
        msg.data = bytes;

        ros.Publish(topic, msg);
    }
}
