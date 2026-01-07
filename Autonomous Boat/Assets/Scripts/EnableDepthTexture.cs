using UnityEngine;

[RequireComponent(typeof(Camera))]
public class EnableDepthTexture : MonoBehaviour
{
    void Awake()
    {
        var cam = GetComponent<Camera>();
        cam.depthTextureMode |= DepthTextureMode.Depth;
        // Or DepthNormals if you also need normals:
        // cam.depthTextureMode |= DepthTextureMode.DepthNormals;
    }
}