Shader "Grid3D/Starfield"
{
    Properties
    {
        _Density("Density", Float) = 60
        _Chance("Star Chance", Range(0, 1)) = 0.15
        _Size("Star Size", Range(0.01, 0.5)) = 0.15
        _Brightness("Brightness", Float) = 1.5
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

            float _Density, _Chance, _Size, _Brightness;

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 direction : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.positionOS.xyz;
                return output;
            }

            float3 Hash33(float3 p)
            {
                p = frac(p * float3(443.897, 441.423, 437.195));
                p += dot(p, p.yxz + 19.19);
                return frac((p.xxy + p.yxx) * p.zyx);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 p = normalize(input.direction) * _Density;
                float3 cell = floor(p);
                float3 roll = Hash33(cell);
                if (roll.x > _Chance) return half4(0, 0, 0, 1);
                float3 center = cell + 0.5 + (Hash33(cell + 17.0) - 0.5) * (1.0 - 2.0 * _Size);
                float star = smoothstep(_Size, 0.0, length(p - center)) * (0.3 + 0.7 * roll.y);
                return half4(star.xxx * _Brightness, 1);
            }
            ENDHLSL
        }
    }
}
