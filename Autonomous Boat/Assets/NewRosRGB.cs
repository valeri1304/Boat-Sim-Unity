using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class RosRgbCompressedPublisher : MonoBehaviour
{
    [Header("Camera")]
    public Camera rgbCam;

    [Header("ROS")]
    public string topic = "/camera/color/image_raw/compressed";
    public string frameId = "camera_color_frame";
    public int fps = 30;
    [Range(1, 100)] public int jpegQuality = 80;

    ROSConnection ros;
    Texture2D cpuTex;
    float nextTime;

    void Start()
    {
        if (rgbCam == null)
        {
            Debug.LogError("RosRgbCompressedPublisher: rgbCam is not assigned.");
            enabled = false;
            return;
        }

        if (rgbCam.targetTexture == null)
        {
            Debug.LogError("RosRgbCompressedPublisher: rgbCam must have a TargetTexture.");
            enabled = false;
            return;
        }

        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<CompressedImageMsg>(topic);

        RenderTexture rt = rgbCam.targetTexture;
        cpuTex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
    }

    void Update()
    {
        if (Time.unscaledTime < nextTime)
            return;

        nextTime = Time.unscaledTime + 1f / Mathf.Max(1, fps);

        RenderTexture rt = rgbCam.targetTexture;

        // Ja RenderTexture izmērs mainījies – pārbūvē Texture2D
        if (cpuTex.width != rt.width || cpuTex.height != rt.height)
        {
            cpuTex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        }

        RenderTexture.active = rt;
        cpuTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        cpuTex.Apply(false);
        RenderTexture.active = null;

        byte[] jpg = cpuTex.EncodeToJPG(jpegQuality);

        double t = Time.realtimeSinceStartupAsDouble;
        int sec = (int)t;
        uint nanosec = (uint)((t - sec) * 1e9);

        var stamp = new TimeMsg { sec = sec, nanosec = nanosec };
        var header = new HeaderMsg { stamp = stamp, frame_id = frameId };

        var msg = new CompressedImageMsg
        {
            header = header,
            format = "jpeg",
            data = jpg
        };

        ros.Publish(topic, msg);
    }
}
