using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
///     This script implements a depth renderer feature
///     that can capture a depth image.
///     The camera with tag "DepthCamera" would apply this
///     render pass and capture a depth texture.
/// </summary>
public class DepthRendererFeature : ScriptableRendererFeature 
{
    class RenderPass : ScriptableRenderPass 
    {
        private Material material;
        private RenderTargetHandle tempTexture;
        private RenderTargetIdentifier source;
        private RenderTextureDescriptor descriptor;

        public RenderPass(Material material) : base() 
        {
            this.material = material;
            tempTexture.Init("_TempDepthTexture");
        }

        public void SetSource(RenderTargetIdentifier source) 
        {
            this.source = source;
        }

        public override void Execute(
            ScriptableRenderContext context, ref RenderingData renderingData
        ) 
        {
            // Init buffer
            CommandBuffer cmd = CommandBufferPool.Get("DepthFeature");
            // Acquire a temporary texture  
            cmd.GetTemporaryRT(
                tempTexture.id, 
                renderingData.cameraData.cameraTargetDescriptor
            );

            // Blit, applying the depth material
            cmd.Blit(source, tempTexture.Identifier(), material);
            cmd.Blit(tempTexture.Identifier(), source);
            context.ExecuteCommandBuffer(cmd);
            // End
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd) 
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    private RenderPass renderPass;

    // Init renderer feature
    public override void Create() 
    {
        // Get a depth render pass
        var material = new Material(Shader.Find("Custom/DepthShader"));
        renderPass = new RenderPass(material);

        // Use the depth render pass after rendering
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // Run at every frame
    public override void AddRenderPasses(
        ScriptableRenderer renderer, ref RenderingData renderingData
    ) 
    {
        // Only apply to "DepthCamera"
        if (renderingData.cameraData.camera.tag == "DepthCamera")
        {
            renderPass.SetSource(renderer.cameraColorTarget);
            renderer.EnqueuePass(renderPass);
        }
    }
}
