using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DepthBlit : MonoBehaviour
{
    public Shader linearDepthShader;
    Material mat;

    void Start()
    {
        if (linearDepthShader == null)
        {
            Debug.LogError("DepthBlit: assign the LinearDepthToRFloat shader in Inspector.");
            enabled = false;
            return;
        }

        mat = new Material(linearDepthShader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // dest = Depth Camera's Target Texture (your DEPTH_720p RT)
        Graphics.Blit(src, dest, mat);
    }
}
