using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class RosImagePublisher : MonoBehaviour
{
    public Camera rgbCamera;
    public string topicName = "/rgb";
    public string frameId = "camera_color_frame";
    public int width = 640;
    public int height = 480;
    public float publishRate = 30f;

    private ROSConnection ros;
    private ImageMsg imageMsg;
    private Texture2D texture;
    private RenderTexture renderTexture;

    private float publishTimer = 0f;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);

        renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        texture = new Texture2D(width, height, TextureFormat.RGB24, false);

        imageMsg = new ImageMsg
        {
            header = new HeaderMsg
            {
                stamp = new TimeMsg { sec = 0, nanosec = 0 },
                frame_id = frameId
            },
            height = (uint)height,
            width = (uint)width,
            encoding = "rgb8",
            is_bigendian = 0,
            step = (uint)(width * 3),
            data = new byte[width * height * 3]
        };
    }

    void Update()
    {
        publishTimer += Time.deltaTime;
        if (publishTimer < 1f / publishRate)
            return;

        publishTimer = 0f;
        PublishImage();
    }

    void PublishImage()
    {
        rgbCamera.targetTexture = renderTexture;
        rgbCamera.Render();

        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        RenderTexture.active = null;
        rgbCamera.targetTexture = null;

        byte[] rawData = texture.GetRawTextureData();
        imageMsg.data = rawData;

        ros.Publish(topicName, imageMsg);
    }
}
