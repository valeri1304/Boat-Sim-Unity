using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class RosRgbCompressedPublisher : MonoBehaviour
{
    public Camera rgbCam;
    public string topic = "/camera/color/image_raw/compressed";
    public string frameId = "camera_color_frame";

    public int width = 1280;
    public int height = 720;
    public int fps = 30;
    [Range(1, 100)] public int jpegQuality = 80;

    ROSConnection ros;
    Texture2D cpuTex;
    float nextTime;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<CompressedImageMsg>(topic);

        if (rgbCam == null)
            throw new Exception("RosRgbCompressedPublisher: Assign rgbCam in Inspector.");

        if (rgbCam.targetTexture == null)
            throw new Exception("RosRgbCompressedPublisher: RGB camera needs a Target Texture (RenderTexture).");

        cpuTex = new Texture2D(width, height, TextureFormat.RGB24, false);
    }

    void Update()
    {
        if (Time.unscaledTime < nextTime) return;
        nextTime = Time.unscaledTime + 1f / Mathf.Max(1, fps);

        RenderTexture rt = rgbCam.targetTexture;

        RenderTexture.active = rt;
        cpuTex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        cpuTex.Apply(false);
        RenderTexture.active = null;

        byte[] jpg = cpuTex.EncodeToJPG(jpegQuality);

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

        var msg = new CompressedImageMsg();
        msg.header = header;
        msg.format = "jpeg";
        msg.data = jpg;

        ros.Publish(topic, msg);
    }
}
