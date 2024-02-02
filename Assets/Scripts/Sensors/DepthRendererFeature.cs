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
    class DepthRenderPass : ScriptableRenderPass 
    {
        private Material material;
        private RTHandle target;

        public DepthRenderPass(Material material) : base() 
        {
            this.material = material;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public override void OnCameraSetup(
            CommandBuffer cmd, ref RenderingData renderingData
        )
        {
            ConfigureTarget(target);
        }

        public void SetTarget(RTHandle target) 
        {
            this.target = target;
        }

        public override void Execute(
            ScriptableRenderContext context, ref RenderingData renderingData
        ) 
        {
            // Init buffer
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, profilingSampler))
            {
                Blitter.BlitCameraTexture(cmd, target, target, material, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }
    }

    private DepthRenderPass renderPass;

    // Init renderer feature
    public override void Create() 
    {
        // Get a depth render pass
        var material = new Material(Shader.Find("Custom/DepthShader"));
        renderPass = new DepthRenderPass(material);
    }

    // Run at every frame
    public override void AddRenderPasses(
        ScriptableRenderer renderer, ref RenderingData renderingData
    ) 
    {
        // Only apply to "DepthCamera"
        if (renderingData.cameraData.camera.tag == "DepthCamera")
        {
            renderer.EnqueuePass(renderPass);
        }
    }

    public override void SetupRenderPasses(
        ScriptableRenderer renderer, in RenderingData renderingData
    )
    {
        // Only apply to "DepthCamera"
        if (renderingData.cameraData.camera.tag == "DepthCamera")
        {
            renderPass.SetTarget(renderer.cameraColorTargetHandle);
        }
    }
}
