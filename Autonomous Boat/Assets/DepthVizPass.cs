using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[System.Serializable]
class DepthVizPass : CustomPass
{
    public float nearPlane = 0.1f;
    public float farPlane = 100f;

    Material mat;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        mat = new Material(Shader.Find("Hidden/DepthVizShader"));
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (mat == null) return;

        mat.SetFloat("_Near", nearPlane);
        mat.SetFloat("_Far", farPlane);

        CoreUtils.DrawFullScreen(ctx.cmd, mat);
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(mat);
    }
}
