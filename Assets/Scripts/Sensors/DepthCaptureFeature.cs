using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthCaptureFeature : ScriptableRendererFeature
{
    class DepthCapturePass : ScriptableRenderPass
    {
        private RenderTargetHandle depthTextureHandle; 

        public DepthCapturePass()
        {
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
            depthTextureHandle.Init("_CameraDepthTexture");
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            var depthDescriptor = cameraTextureDescriptor;
            depthDescriptor.colorFormat = RenderTextureFormat.RFloat;
            cmd.GetTemporaryRT(depthTextureHandle.id, depthDescriptor, FilterMode.Point);
            ConfigureTarget(depthTextureHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("Depth Capture Pass")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(new ShaderTagId("UniversalForward"), ref renderingData, sortFlags);
                var filterSettings = new FilteringSettings(RenderQueueRange.opaque);

                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings);

                cmd.SetGlobalTexture("_CameraDepthTexture", depthTextureHandle.Identifier());
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(depthTextureHandle.id);
        }
    }

    DepthCapturePass depthCapturePass;

    public override void Create()
    {
        depthCapturePass = new DepthCapturePass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(depthCapturePass);
    }
}