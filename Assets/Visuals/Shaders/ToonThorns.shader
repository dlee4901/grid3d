Shader "Grid3D/ToonThorns"
{
    Properties
    {
        _OuterColor("Outline", Color) = (0.07, 0.13, 0.05, 1)
        _MidColor("Body", Color) = (0.2, 0.36, 0.1, 1)
        _InnerColor("Highlight", Color) = (0.38, 0.58, 0.16, 1)
        _TipColor("Tip", Color) = (0.86, 0.8, 0.56, 1)
        _MidBand("Body Band", Range(0, 1)) = 0.7
        _InnerBand("Highlight Band", Range(0, 1)) = 0.3
        _TipStart("Tip Start", Range(0, 1)) = 0.78
        _Speed("Sway Speed", Float) = 1.2
        _Sway("Sway", Range(0, 1)) = 0.06
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
                float4 _OuterColor, _MidColor, _InnerColor, _TipColor;
                float _MidBand, _InnerBand, _TipStart, _Speed, _Sway;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float phase : TEXCOORD1; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 center = TransformObjectToWorld(float3(0, 0, 0));
                float2 scale = float2(length(GetObjectToWorldMatrix()._m00_m10_m20), length(GetObjectToWorldMatrix()._m01_m11_m21));
                float3 viewCenter = TransformWorldToView(center);
                float3 viewPos = viewCenter + float3(input.positionOS.xy * scale, 0);
                output.positionCS = TransformWViewToHClip(viewPos);
                output.uv = input.uv;
                output.phase = frac(dot(center, float3(12.9898, 78.233, 37.719))) * 17.0;
                return output;
            }

            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y * _Speed + input.phase;
                float x = input.uv.x;
                float y = input.uv.y;

                float best = 10.0;
                float bestAlong = 0.0;
                [unroll]
                for (int i = 0; i < 3; i++)
                {
                    float fi = i;
                    float baseX = 0.22 + 0.28 * fi + (Hash(float2(input.phase, fi)) - 0.5) * 0.1;
                    float height = lerp(0.6, 1.0, Hash(float2(fi + 3.1, input.phase)));
                    float lean = (baseX - 0.5) * 0.8 + sin(t + fi * 1.7) * _Sway;
                    float along = y / height;
                    float halfWidth = 0.12 * pow(saturate(1.0 - along), 1.3) + 1e-3;
                    float axis = baseX + lean * along * along;
                    float d = along < 1.0 ? abs(x - axis) / halfWidth : 10.0;
                    if (d < best)
                    {
                        best = d;
                        bestAlong = along;
                    }
                }

                float aa = max(fwidth(best), 1e-3);
                float alpha = saturate((1.0 - best) / aa);
                clip(alpha - 0.5);

                float3 color = _OuterColor.rgb;
                color = lerp(color, _MidColor.rgb, step(best, _MidBand));
                color = lerp(color, _InnerColor.rgb, step(best, _InnerBand) * step(bestAlong, 0.6));
                color = lerp(color, _TipColor.rgb, step(_TipStart, bestAlong) * step(best, _MidBand));
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
