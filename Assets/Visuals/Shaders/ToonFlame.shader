Shader "Grid3D/ToonFlame"
{
    Properties
    {
        [HDR] _OuterColor("Outer", Color) = (2.0, 0.35, 0.1, 1)
        [HDR] _MidColor("Mid", Color) = (2.5, 1.0, 0.15, 1)
        [HDR] _InnerColor("Inner", Color) = (3.0, 2.4, 0.8, 1)
        _MidBand("Mid Band", Range(0, 1)) = 0.7
        _InnerBand("Inner Band", Range(0, 1)) = 0.4
        _Speed("Flicker Speed", Float) = 2.5
        _Wobble("Wobble", Range(0, 1)) = 0.35
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
                float4 _OuterColor, _MidColor, _InnerColor;
                float _MidBand, _InnerBand, _Speed, _Wobble;
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

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(Hash(i), Hash(i + float2(1, 0)), u.x),
                            lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), u.x), u.y);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float t = _Time.y * _Speed + input.phase;
                float y = input.uv.y;
                float x = (input.uv.x - 0.5) * 2.0;

                float n = Noise(float2(x * 1.5, y * 2.5 - t)) - 0.5;
                x += n * _Wobble * y * 2.0;

                float width = pow(saturate(1.0 - y), 0.75) * saturate(y * 5.0 + 0.35);
                float d = abs(x) / max(width, 1e-3);
                d += (Noise(float2(x * 3.0, y * 5.0 - t * 1.7)) - 0.5) * 0.35 * y;

                float aa = max(fwidth(d), 1e-3);
                float alpha = saturate((1.0 - d) / aa);
                clip(alpha - 0.5);

                float3 color = _OuterColor.rgb;
                color = lerp(color, _MidColor.rgb, step(d, _MidBand));
                color = lerp(color, _InnerColor.rgb, step(d, _InnerBand) * step(y, 0.65));
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
