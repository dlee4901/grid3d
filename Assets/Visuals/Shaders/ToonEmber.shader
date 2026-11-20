Shader "Grid3D/ToonEmber"
{
    Properties
    {
        [HDR] _HotColor("Hot", Color) = (3.0, 2.2, 0.6, 1)
        [HDR] _CoolColor("Cool", Color) = (2.0, 0.45, 0.1, 1)
        _Rise("Rise Height", Float) = 0.45
        _Spread("Spread", Float) = 0.15
        _Speed("Speed", Float) = 0.6
    }

    SubShader
    {
        Tags { "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" "RenderPipeline" = "UniversalPipeline" "DisableBatching" = "True" }
        Cull Off
        ZWrite On

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _HotColor, _CoolColor;
                float _Rise, _Spread, _Speed;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float life : TEXCOORD1; };

            float3 Hash3(float3 p)
            {
                p = float3(dot(p, float3(127.1, 311.7, 74.7)), dot(p, float3(269.5, 183.3, 246.1)), dot(p, float3(113.5, 271.9, 124.6)));
                return frac(sin(p) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 origin = TransformObjectToWorld(float3(0, 0, 0));
                float3 h = Hash3(origin);
                float life = frac(_Time.y * _Speed * (0.7 + 0.6 * h.z) + h.x);
                float3 drift = float3(h.y - 0.5, 0, h.z - 0.5) * 2.0 * _Spread * life;
                drift.x += sin(life * 9.0 + h.x * 6.28) * 0.03;
                float3 center = origin + float3(0, life * _Rise, 0) + drift;
                float size = length(GetObjectToWorldMatrix()._m00_m10_m20) * (1.0 - life);
                float3 viewPos = TransformWorldToView(center) + float3(input.positionOS.xy * size, 0);
                output.positionCS = TransformWViewToHClip(viewPos);
                output.uv = input.uv;
                output.life = life;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = abs(input.uv - 0.5) * 2.0;
                float d = p.x + p.y;
                float aa = max(fwidth(d), 1e-3);
                clip(saturate((1.0 - d) / aa) - 0.5);
                return half4(lerp(_HotColor.rgb, _CoolColor.rgb, step(0.45, input.life)), 1);
            }
            ENDHLSL
        }
    }
}
