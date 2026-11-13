Shader "Grid3D/Aurora"
{
    Properties
    {
        [HDR] _Color("Color", Color) = (1, 1, 1, 1)
        _Intensity("Intensity", Float) = 10
        _Falloff("Vertical Falloff", Float) = 2
        _WallSharpness("Wall Sharpness", Float) = 25
        _StreakCount("Streak Count", Float) = 9
        _Speed("Speed", Float) = 0.6
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "Aurora"
            Tags { "LightMode" = "UniversalForward" }

            Blend One One
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define STEPS 12

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Intensity;
                float _Falloff;
                float _WallSharpness;
                float _StreakCount;
                float _Speed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 cameraOS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                output.cameraOS = TransformWorldToObject(_WorldSpaceCameraPos);
                return output;
            }

            float Density(float3 p, float time)
            {
                float height = saturate(p.y + 0.5);
                float toWall = max(0.5 - max(abs(p.x), abs(p.z)), 0.0);
                float wall = exp(-toWall * _WallSharpness);

                float angle = atan2(p.z, p.x);
                float sway = sin(height * 3.0 - time * 2.0) * 1.5;
                float wave = sin(angle * _StreakCount + time * 1.3 + sway);
                float ripple = sin(angle * (_StreakCount * 2.0 + 1.0) - time * 0.7 + height * 2.0);
                float streaks = saturate(0.55 + 0.3 * wave + 0.15 * ripple);

                float fade = pow(1.0 - height, _Falloff);
                return wall * streaks * fade;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 entry = input.positionOS;
                float3 dir = entry - input.cameraOS;
                float3 invDir = 1.0 / (abs(dir) < 1e-6 ? 1e-6 : dir);
                float3 tNear = (-0.5 - input.cameraOS) * invDir;
                float3 tFar = (0.5 - input.cameraOS) * invDir;
                float3 tExit = max(tNear, tFar);
                float3 exit = input.cameraOS + dir * min(tExit.x, min(tExit.y, tExit.z));
                float3 segment = exit - entry;

                float time = _Time.y * _Speed + dot(UNITY_MATRIX_M._m03_m13_m23, float3(1.3, 0.0, 2.1));
                float total = 0.0;
                [unroll]
                for (int i = 0; i < STEPS; i++)
                    total += Density(entry + segment * ((i + 0.5) / STEPS), time);
                total *= length(segment) / STEPS;

                return half4(_Color.rgb * _Intensity * total, 0.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
