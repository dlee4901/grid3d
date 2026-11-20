Shader "Grid3D/Sky"
{
    Properties
    {
        _ZenithColor("Pole Color", Color) = (0.25, 0.53, 0.91, 1)
        _HorizonColor("Equator Color", Color) = (0.56, 0.80, 0.98, 1)
        _GradientPower("Gradient Power", Float) = 0.7

        _SunDirection("Sun Direction", Vector) = (0.4, 0.6, 0.3, 0)
        _SunColor("Sun Color", Color) = (1.0, 0.96, 0.78, 1)
        _SunSize("Sun Size", Range(0.01, 0.3)) = 0.06
        _SunHaloColor("Sun Halo", Color) = (0.80, 0.90, 1.0, 1)
        _SunHaloSize("Sun Halo Size", Range(0.01, 0.5)) = 0.12

        _CloudLitColor("Cloud Lit", Color) = (1, 1, 1, 1)
        _CloudShadeColor("Cloud Shade", Color) = (0.80, 0.86, 0.96, 1)
        _CloudBottomColor("Cloud Underside", Color) = (0.68, 0.76, 0.91, 1)
        _CloudLineColor("Cloud Line", Color) = (0.45, 0.56, 0.78, 1)
        _CloudLightDir("Cloud Light Direction", Vector) = (-0.45, 0.65, 0.6, 0)
        _CloudScale("Cloud Scale", Float) = 2.5
        _CloudCoverage("Cloud Coverage", Range(0, 1)) = 0.6
        _SizeVariation("Size Variation", Range(0, 1)) = 0.8
        _LineWidth("Line Width (px)", Range(0.5, 4)) = 1.25
        _ShadeThreshold("Shade Threshold", Range(0, 1)) = 0.45
        _UndersideHeight("Underside Height", Range(0, 0.5)) = 0.12
        _EdgeWobble("Edge Wobble", Range(0, 0.1)) = 0.025
        _CloudSpeed("Cloud Speed", Float) = 0.01
    }

    SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _ZenithColor, _HorizonColor;
                float _GradientPower;
                float4 _SunDirection, _SunColor, _SunHaloColor;
                float _SunSize, _SunHaloSize;
                float4 _CloudLitColor, _CloudShadeColor, _CloudBottomColor, _CloudLineColor, _CloudLightDir;
                float _CloudScale, _CloudCoverage, _SizeVariation, _LineWidth, _ShadeThreshold, _UndersideHeight, _EdgeWobble, _CloudSpeed;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 direction : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
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

            struct CloudPaint
            {
                float coverage;
                float solid;
                float3 normal;
                float outline;
                float height;
            };

            void PaintPuff(float2 p, float2 center, float r, float baseY, float width, float lw, inout CloudPaint o)
            {
                float dist = length(p - center);
                float inside = min(r - dist, p.y - baseY);
                if (inside <= -lw) return;
                if (inside > 0.0)
                {
                    float2 d = (p - center) / r;
                    o.solid = 1.0;
                    o.normal = float3(d, sqrt(saturate(1.0 - dot(d, d))));
                    o.height = (p.y - baseY) / width;
                    o.outline = 1.0 - smoothstep(lw * 0.5, lw * 1.5, inside);
                }
                o.coverage = max(o.coverage, saturate(inside / lw + 1.0));
            }

            void PaintCloud(float2 p, float2 c, float lw, inout CloudPaint o)
            {
                float3 h = float3(Hash(c), Hash(c + 17.31), Hash(c + 41.77));
                if (h.x > _CloudCoverage) return;

                float size = lerp(0.5, h.y * h.y * 1.6 + 0.12, _SizeVariation);
                float width = 0.18 + 0.32 * size;
                float2 base = c + float2(0.5 + (h.z - 0.5) * 0.3, 0.22 + (h.y - 0.5) * 0.15);

                for (int k = 0; k < 3; k++)
                {
                    float hk = Hash(c + k * 7.31 + 3.3);
                    float t = (k - 1) * 0.55;
                    float r = width * (0.3 + 0.14 * hk) * (1.0 - abs(t) * 0.35);
                    float2 center = float2(base.x + t * width + (hk - 0.5) * 0.12 * width, base.y + width * 0.42 + r * 0.45);
                    PaintPuff(p, center, r, base.y, width, lw, o);
                }

                for (int m = 0; m < 6; m++)
                {
                    float t = ((m % 2) == 0 ? -1.0 : 1.0) * (1.0 - 0.4 * floor(m * 0.5));
                    float hk = Hash(c + m * 3.13);
                    float r = width * (0.22 + 0.3 * (1.0 - t * t)) * (0.8 + 0.4 * hk);
                    float2 center = float2(base.x + t * width * 0.85, base.y + r * 0.5);
                    PaintPuff(p, center, r, base.y, width, lw, o);
                }
            }

            float4 ShadeClouds(float2 p)
            {
                float lw = length(fwidth(p)) * _LineWidth;
                p += (float2(Noise(p * 9.0), Noise(p * 9.0 + 13.7)) - 0.5) * _EdgeWobble;

                CloudPaint o;
                o.coverage = 0.0;
                o.solid = 0.0;
                o.normal = float3(0, 0, 1);
                o.outline = 0.0;
                o.height = 1.0;

                float2 cell = floor(p);
                for (int cy = 1; cy >= -1; cy--)
                for (int cx = -1; cx <= 1; cx++)
                    PaintCloud(p, cell + float2(cx, cy), lw, o);

                float3 light = normalize(_CloudLightDir.xyz);
                float lit = dot(o.normal, light) * 0.5 + 0.5;
                float band = smoothstep(_ShadeThreshold - 0.02, _ShadeThreshold + 0.02, lit);
                float3 color = lerp(_CloudShadeColor.rgb, _CloudLitColor.rgb, band);
                color = lerp(_CloudBottomColor.rgb, color, smoothstep(_UndersideHeight - 0.02, _UndersideHeight + 0.02, o.height));
                color = lerp(color, _CloudLineColor.rgb, max(o.outline, 1.0 - o.solid));
                return float4(color * o.coverage, o.coverage);
            }

            float Edge(float value, float threshold)
            {
                float aa = max(fwidth(value), 1e-4);
                return saturate((value - threshold) / aa + 0.5);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.direction);
                float3 sunDir = normalize(_SunDirection.xyz);

                float3 sky = lerp(_HorizonColor.rgb, _ZenithColor.rgb, pow(abs(dir.y), _GradientPower));
                float sunAngle = acos(clamp(dot(dir, sunDir), -1.0, 1.0));
                sky = lerp(sky, _SunHaloColor.rgb, (1.0 - Edge(sunAngle, _SunHaloSize)) * 0.5);
                sky = lerp(sky, _SunColor.rgb, 1.0 - Edge(sunAngle, _SunSize));

                float3 a = abs(dir);
                float3 w = pow(a, 12.0);
                w /= (w.x + w.y + w.z);
                float2 drift = float2(_Time.y * _CloudSpeed, 0.0);
                float4 cx = ShadeClouds(dir.zy / max(a.x, 1e-3) * _CloudScale + drift + 31.0);
                float4 cy = ShadeClouds(dir.xz / max(a.y, 1e-3) * _CloudScale + drift + 63.0);
                float4 cz = ShadeClouds(dir.xy / max(a.z, 1e-3) * _CloudScale + drift);
                float4 cloud = cx * w.x + cy * w.y + cz * w.z;

                return half4(sky * (1.0 - cloud.a) + cloud.rgb, 1);
            }
            ENDHLSL
        }
    }
}
