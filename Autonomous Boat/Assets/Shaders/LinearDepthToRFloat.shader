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
                float raw = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float eye = LinearEyeDepth(raw);
                return float4(eye, 0, 0, 1);
            }
            ENDCG
        }
    }
}
