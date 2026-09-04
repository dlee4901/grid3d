Shader "Grid3D/Cube Edge Outline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _Thickness("Outline Thickness (world units)", Float) = 0.02
        _Smoothing("Edge Smoothing", Range(0, 2)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "DisableBatching" = "True"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Offset -1, -1
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _Thickness;
                float _Smoothing;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 normalOS : TEXCOORD1;
                float3 scale : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;
                output.scale = float3(
                    length(UNITY_MATRIX_M._m00_m10_m20),
                    length(UNITY_MATRIX_M._m01_m11_m21),
                    length(UNITY_MATRIX_M._m02_m12_m22));
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 faceAxis = abs(normalize(input.normalOS));
                float3 distances = (0.5 - abs(input.positionOS)) * input.scale + faceAxis * 1e4;
                float distanceToEdge = min(distances.x, min(distances.y, distances.z));

                float aa = max(fwidth(distanceToEdge) * _Smoothing, 1e-5);
                float outline = 1.0 - smoothstep(_Thickness - aa, _Thickness + aa, distanceToEdge);

                return half4(_OutlineColor.rgb, _OutlineColor.a * outline);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
