Shader "Hidden/UIBlur"
{
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    float4 _SourceTexel;
    float4 _BlurTexel;
    float _BlurRadius;

    half4 Sample(float2 uv)
    {
        return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);
    }

    half4 Downsample(Varyings input) : SV_Target
    {
        float2 uv = input.texcoord;
        float2 o = _SourceTexel.xy;
        return (Sample(uv + float2(-o.x, -o.y)) + Sample(uv + float2(o.x, -o.y)) +
                Sample(uv + float2(-o.x, o.y)) + Sample(uv + float2(o.x, o.y))) * 0.25;
    }

    half4 Gaussian(float2 uv, float2 direction)
    {
        float2 step = direction * _BlurTexel.xy * _BlurRadius;
        half4 color = Sample(uv) * 0.227027;
        color += (Sample(uv + step * 1.384615) + Sample(uv - step * 1.384615)) * 0.316216;
        color += (Sample(uv + step * 3.230769) + Sample(uv - step * 3.230769)) * 0.070270;
        return color;
    }

    half4 Horizontal(Varyings input) : SV_Target { return Gaussian(input.texcoord, float2(1, 0)); }
    half4 Vertical(Varyings input) : SV_Target { return Gaussian(input.texcoord, float2(0, 1)); }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off Blend Off

        Pass
        {
            Name "Downsample"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Downsample
            ENDHLSL
        }

        Pass
        {
            Name "Horizontal"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Horizontal
            ENDHLSL
        }

        Pass
        {
            Name "Vertical"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Vertical
            ENDHLSL
        }
    }
}
