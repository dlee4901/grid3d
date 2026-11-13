Shader "Grid3D/Infinite Grid"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0, 0, 0, 1)
        [HDR] _LineColor("Line Color", Color) = (0.5, 0.5, 0.5, 1)
        [HDR] _MajorLineColor("Major Line Color", Color) = (0.6, 0.6, 0.6, 1)
        _CellSize("Cell Size (world units)", Float) = 1
        _MajorEvery("Major Line Every (cells)", Float) = 10
        _Thickness("Line Thickness (world units)", Float) = 0.02
        _LineFadeStart("Line Fade Start (distance)", Float) = 30
        _LineFadeEnd("Line Fade End (distance)", Float) = 150
        [HDR] _HorizonColor("Horizon Color", Color) = (2, 2, 2, 1)
        _HorizonStart("Horizon Start (distance)", Float) = 200
        _HorizonEnd("Horizon End (distance)", Float) = 900
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "InfiniteGrid"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            ZTest LEqual
            Offset 1, 1
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _LineColor;
                float4 _MajorLineColor;
                float _CellSize;
                float _MajorEvery;
                float _Thickness;
                float _LineFadeStart;
                float _LineFadeEnd;
                float4 _HorizonColor;
                float _HorizonStart;
                float _HorizonEnd;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float GridMask(float2 coord, float width)
            {
                float2 aa = fwidth(coord);
                float2 toLine = abs(frac(coord + 0.5) - 0.5);
                float halfWidth = 0.5 * width;
                float2 lines = 1.0 - smoothstep(halfWidth, halfWidth + aa, toLine);
                return max(lines.x, lines.y) * saturate(1.0 - max(aa.x, aa.y));
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float cameraDistance = length(input.positionWS.xz - _WorldSpaceCameraPos.xz);
                float fade = 1.0 - smoothstep(_LineFadeStart, _LineFadeEnd, cameraDistance);
                float lineMask = GridMask(input.positionWS.xz / _CellSize, _Thickness / _CellSize) * fade;
                float majorCell = _CellSize * _MajorEvery;
                float majorMask = GridMask(input.positionWS.xz / majorCell, _Thickness / majorCell) * fade;
                float horizon = smoothstep(_HorizonStart, _HorizonEnd, cameraDistance);

                half3 rgb = lerp(_BaseColor.rgb, _LineColor.rgb, lineMask);
                rgb = lerp(rgb, _MajorLineColor.rgb, majorMask);
                rgb = lerp(rgb, _HorizonColor.rgb, horizon);
                return half4(rgb, 1.0);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
