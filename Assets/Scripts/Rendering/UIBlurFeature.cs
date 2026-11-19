using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class UIBlurFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader _shader;
    [SerializeField] private RenderPassEvent _event = RenderPassEvent.BeforeRenderingPostProcessing;
    [SerializeField, Range(1, 8)] private int _downsample = 4;
    [SerializeField, Range(1, 8)] private int _iterations = 3;
    [SerializeField, Range(0.5f, 4f)] private float _radius = 1.5f;

    private Material _material;
    private UIBlurPass _pass;

    public override void Create()
    {
        if (_shader == null) return;
        _material = CoreUtils.CreateEngineMaterial(_shader);
        _pass = new UIBlurPass(_material) { renderPassEvent = _event };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_pass == null || renderingData.cameraData.cameraType != CameraType.Game) return;
        _pass.Setup(_downsample, _iterations, _radius);
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        CoreUtils.Destroy(_material);
    }

    private class UIBlurPass : ScriptableRenderPass
    {
        private static readonly int BlurTextureId = Shader.PropertyToID("_UIBlurTexture");
        private static readonly int SourceTexelId = Shader.PropertyToID("_SourceTexel");
        private static readonly int BlurTexelId = Shader.PropertyToID("_BlurTexel");
        private static readonly int RadiusId = Shader.PropertyToID("_BlurRadius");

        private const int DownsamplePass = 0;
        private const int HorizontalPass = 1;
        private const int VerticalPass = 2;

        private readonly Material _material;
        private RTHandle _result;
        private int _downsample;
        private int _iterations;
        private float _radius;

        private class PassData
        {
            public TextureHandle Source;
            public Material Material;
        }

        public UIBlurPass(Material material)
        {
            _material = material;
            requiresIntermediateTexture = true;
        }

        public void Setup(int downsample, int iterations, float radius)
        {
            _downsample = downsample;
            _iterations = iterations;
            _radius = radius;
        }

        public void Dispose() => _result?.Release();

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            if (resourceData.isActiveTargetBackBuffer) return;

            var full = cameraData.cameraTargetDescriptor;
            var descriptor = full;
            descriptor.width = Mathf.Max(1, full.width / _downsample);
            descriptor.height = Mathf.Max(1, full.height / _downsample);
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;
            RenderingUtils.ReAllocateHandleIfNeeded(ref _result, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_UIBlurTexture");

            _material.SetVector(SourceTexelId, new Vector4(1f / full.width, 1f / full.height));
            _material.SetVector(BlurTexelId, new Vector4(1f / descriptor.width, 1f / descriptor.height));
            _material.SetFloat(RadiusId, _radius);

            var result = renderGraph.ImportTexture(_result);
            var scratch = UniversalRenderer.CreateRenderGraphTexture(renderGraph, descriptor, "_UIBlurScratch", false, FilterMode.Bilinear, TextureWrapMode.Clamp);

            renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(resourceData.activeColorTexture, result, _material, DownsamplePass), "UI Blur Downsample");
            for (var i = 0; i < _iterations; i++)
            {
                renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(result, scratch, _material, HorizontalPass), "UI Blur Horizontal");
                AddFinalPass(renderGraph, scratch, result);
            }
        }

        private void AddFinalPass(RenderGraph renderGraph, TextureHandle source, TextureHandle destination)
        {
            using var builder = renderGraph.AddRasterRenderPass<PassData>("UI Blur Vertical", out var data);
            data.Source = source;
            data.Material = _material;
            builder.UseTexture(source);
            builder.SetRenderAttachment(destination, 0);
            builder.SetGlobalTextureAfterPass(destination, BlurTextureId);
            builder.AllowPassCulling(false);
            builder.SetRenderFunc(static (PassData passData, RasterGraphContext context) =>
                Blitter.BlitTexture(context.cmd, passData.Source, new Vector4(1f, 1f, 0f, 0f), passData.Material, VerticalPass));
        }
    }
}
