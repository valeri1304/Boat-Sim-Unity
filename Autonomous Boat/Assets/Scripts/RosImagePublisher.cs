using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;
using RosMessageTypes.BuiltinInterfaces;

public class RosImagePublisher : MonoBehaviour
{
    [Header("Camera")]
    public Camera rgbCamera;

    [Header("ROS")]
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
        // ROS
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImageMsg>(topicName);

        // Render targets
        renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        texture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Sagatavo ROS Image ziņu
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
        // Renderē kameru uz RenderTexture
        rgbCamera.targetTexture = renderTexture;
        rgbCamera.Render();

        // Nolasa pikseļus no GPU
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        RenderTexture.active = null;
        rgbCamera.targetTexture = null;

        // Iegūst RGB datus
        byte[] src = texture.GetRawTextureData();
        byte[] flipped = new byte[src.Length];

        int stride = width * 3; // rgb8 = 3 baiti uz pikseli

        // ===== 180° APGRIEŠANA =====
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcIndex = y * stride + x * 3;

                int dstIndex =
                    (height - 1 - y) * stride +
                    (width - 1 - x) * 3;

                flipped[dstIndex + 0] = src[srcIndex + 0];
                flipped[dstIndex + 1] = src[srcIndex + 1];
                flipped[dstIndex + 2] = src[srcIndex + 2];
            }
        }

        imageMsg.data = flipped;

        // Publicē uz ROS
        ros.Publish(topicName, imageMsg);
    }
}
