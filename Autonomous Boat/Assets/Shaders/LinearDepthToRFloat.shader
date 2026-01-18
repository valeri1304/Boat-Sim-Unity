Shader "Hidden/LinearDepthToRFloat"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _CameraDepthTexture;

            float4 frag(v2f_img i) : SV_Target
            {
                // Raw depth (non-linear)
                float raw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);

                // Linear depth in "eye space" distance (Unity units)
                float eye = LinearEyeDepth(raw);

                // Put depth in meters into R channel (assuming 1 Unity unit = 1 meter)
                return float4(eye, 0, 0, 1);
            }
            ENDCG
        }
    }
}
