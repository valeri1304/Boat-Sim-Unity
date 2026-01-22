using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;

using RosMessageTypes.Sensor;
using RosMessageTypes.Std;                 
using RosMessageTypes.BuiltinInterfaces;
public class RosDepthMono8Publisher : MonoBehaviour
{
    public Camera depthCam;

    [Header("ROS")]
    public string topic = "/camera/depth/image_mono8";
    public string frameId = "camera_depth_frame";
    public int fps = 15;

    [Header("Visualization")]
    public float maxMeters = 20f;

    ROSConnection ros;
    Texture2D depthCpu;
    float nextTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topic);

        if (depthCam == null)
            throw new Exception("Assign depthCam");

        if (depthCam.targetTexture == null)
            throw new Exception("DepthCam needs a RenderTexture");

        var rt = depthCam.targetTexture;
        depthCpu = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false, true);
    }

    void Update()
    {
        if (Time.unscaledTime < nextTime) return;
        nextTime = Time.unscaledTime + 1f / Mathf.Max(1, fps);

        RenderTexture rt = depthCam.targetTexture;

        // Read depth
        RenderTexture.active = rt;
        depthCpu.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        depthCpu.Apply(false);
        RenderTexture.active = null;

        var depthFloats = depthCpu.GetRawTextureData<float>();

        int width = rt.width;
        int height = rt.height;

        byte[] mono = new byte[width * height];
        float inv = (maxMeters > 0.001f) ? (255f / maxMeters) : 0f;

        // 🔁 180° ROTĀCIJA (X + Y flip)
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int src = y * width + x;
                int dst = (height - 1 - y) * width + (width - 1 - x);

                float d = depthFloats[src];
                if (float.IsNaN(d) || float.IsInfinity(d) || d <= 0f)
                {
                    mono[dst] = 0;
                    continue;
                }

                float v = d * inv;
                mono[dst] = (byte)Mathf.Clamp(v, 0f, 255f);
            }
        }

        // ROS time
        double t = Time.realtimeSinceStartupAsDouble;
        var stamp = new TimeMsg
        {
            sec = (int)t,
            nanosec = (uint)((t - (int)t) * 1e9)
        };

        var msg = new ImageMsg
        {
            header = new HeaderMsg { frame_id = frameId, stamp = stamp },
            height = (uint)height,
            width = (uint)width,
            encoding = "mono8",
            is_bigendian = 0,
            step = (uint)width,
            data = mono
        };

        ros.Publish(topic, msg);
    }
}
