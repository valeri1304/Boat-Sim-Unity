Shader "Hidden/DepthVizShader"
{
    SubShader
    {
        Tags { "RenderPipeline"="HDRenderPipeline" }

        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

            float _Near;
            float _Far;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return o;
            }

            float Frag(Varyings i) : SV_Target
            {
                float depth = LinearEyeDepth(
                    SampleCameraDepth(i.uv),
                    _ZBufferParams
                );

                float norm = saturate((depth - _Near) / (_Far - _Near));

                return norm; // <-- TUVS = TUMŠS, TĀLS = BALTS
            }
            ENDHLSL
        }
    }
}
