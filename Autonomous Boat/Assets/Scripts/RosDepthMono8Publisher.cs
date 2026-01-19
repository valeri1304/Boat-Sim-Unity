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
    public float maxMeters = 20f;   // 0..maxMeters -> 0..255 (tālāk par max -> 255)

    ROSConnection ros;
    Texture2D depthCpu;            // RFloat readback
    Texture2D monoCpu;             // RGBA32 for Encode/bytes
    float nextTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topic);

        if (depthCam == null) throw new Exception("RosDepthMono8Publisher: Assign depthCam.");
        if (depthCam.targetTexture == null) throw new Exception("RosDepthMono8Publisher: depthCam needs a Target Texture.");

        var rt = depthCam.targetTexture;

        depthCpu = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false, true);
        monoCpu = new Texture2D(rt.width, rt.height, TextureFormat.R8, false, true); // 1 kanāls
    }

    void Update()
    {
        if (Time.unscaledTime < nextTime) return;
        nextTime = Time.unscaledTime + 1f / Mathf.Max(1, fps);

        RenderTexture rt = depthCam.targetTexture;

        // ja mainās RT izmērs
        if (depthCpu.width != rt.width || depthCpu.height != rt.height)
        {
            depthCpu = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false, true);
            monoCpu = new Texture2D(rt.width, rt.height, TextureFormat.R8, false, true);
        }

        // 1) nolasa depth floatus
        RenderTexture.active = rt;
        depthCpu.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        depthCpu.Apply(false);
        RenderTexture.active = null;

        var depthFloats = depthCpu.GetRawTextureData<float>();

        // 2) pārveido uz mono8
        // 0m -> 0 (melns), maxMeters -> 255 (balts)
        // NaN/inf/neg -> 0
        int n = depthFloats.Length;
        byte[] mono = new byte[n];

        float inv = (maxMeters > 0.001f) ? (255f / maxMeters) : 0f;

        for (int i = 0; i < n; i++)
        {
            float d = depthFloats[i];
            if (float.IsNaN(d) || float.IsInfinity(d) || d <= 0f) { mono[i] = 0; continue; }

            float v = d * inv;
            if (v > 255f) v = 255f;
            mono[i] = (byte)v;
        }

        // 3) ROS header
        double t = Time.realtimeSinceStartupAsDouble;
        int sec = (int)t;
        uint nanosec = (uint)((t - sec) * 1e9);

        var stamp = new TimeMsg { sec = sec, nanosec = nanosec };
        var header = new HeaderMsg { stamp = stamp, frame_id = frameId };

        // 4) ImageMsg (mono8)
        var msg = new ImageMsg
        {
            header = header,
            height = (uint)rt.height,
            width = (uint)rt.width,
            encoding = "mono8",
            is_bigendian = 0,
            step = (uint)rt.width, // 1 baits uz pikseli
            data = mono
        };

        ros.Publish(topic, msg);
    }
}
