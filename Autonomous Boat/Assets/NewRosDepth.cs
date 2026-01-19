using System;
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
    public int fps = 30;

    ROSConnection ros;
    Texture2D depthCpu;
    float nextTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topic);

        if (depthCam == null) throw new Exception("Assign depthCam in Inspector.");
        if (depthCam.targetTexture == null) throw new Exception("Depth camera needs a Target Texture.");

        var rt = depthCam.targetTexture;
        depthCpu = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false, true);
    }

    void Update()
    {
        if (Time.unscaledTime < nextTime) return;
        nextTime = Time.unscaledTime + 1f / Mathf.Max(1, fps);

        RenderTexture rt = depthCam.targetTexture;

        // ja RT izmers mainas runtime, pārtaisa Texture2D
        if (depthCpu.width != rt.width || depthCpu.height != rt.height)
            depthCpu = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false, true);

        RenderTexture.active = rt;
        depthCpu.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        depthCpu.Apply(false);
        RenderTexture.active = null;

        var floats = depthCpu.GetRawTextureData<float>();

        byte[] bytes = new byte[floats.Length * sizeof(float)];
        Buffer.BlockCopy(floats.ToArray(), 0, bytes, 0, bytes.Length);

        // stamp
        double t = Time.realtimeSinceStartupAsDouble;
        int sec = (int)t;
        uint nanosec = (uint)((t - sec) * 1e9);

        var stamp = new TimeMsg { sec = sec, nanosec = nanosec };
        var header = new HeaderMsg { stamp = stamp, frame_id = frameId };

        // 32FC1 -> step = width * 4
        var msg = new ImageMsg
        {
            header = header,
            height = (uint)rt.height,
            width = (uint)rt.width,
            encoding = "32FC1",
            is_bigendian = 0,
            step = (uint)(rt.width * 4),
            data = bytes
        };

        ros.Publish(topic, msg);
    }
}
