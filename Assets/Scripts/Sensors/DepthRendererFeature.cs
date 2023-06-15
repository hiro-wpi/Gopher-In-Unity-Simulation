using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthRendererFeature : ScriptableRendererFeature
{
    class DepthRenderPass : ScriptableRenderPass
    {
        private RenderTargetHandle depthTexture;
        private RenderTextureDescriptor descriptor;
        private Material depthMaterial;

        public DepthRenderPass(Material depthMaterial)
        {
            this.depthMaterial = depthMaterial;
            depthTexture.Init("_CameraDepthTexture");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            descriptor = cameraTextureDescriptor;
            descriptor.colorFormat = RenderTextureFormat.ARGB32;
            cmd.GetTemporaryRT(depthTexture.id, descriptor, FilterMode.Point);
            ConfigureTarget(depthTexture.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("DepthPass");
            using (new ProfilingScope(cmd, new ProfilingSampler("DepthPass")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(new ShaderTagId("DepthOnly"), ref renderingData, sortFlags);
                drawSettings.overrideMaterial = depthMaterial;

                var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(depthTexture.id);
        }
    }

    DepthRenderPass depthPass;
    Material depthMaterial;

    public override void Create()
    {
        depthMaterial = new Material(Shader.Find("Custom/DepthShader2"));
        depthPass = new DepthRenderPass(depthMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(depthPass);
    }
}