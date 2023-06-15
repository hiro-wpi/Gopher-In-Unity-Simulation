using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TestDepth : MonoBehaviour
{
    public Camera cam;
    public Shader redShader;
    private Material redMaterial;
    private CommandBuffer commandBuffer;
    public RenderTexture renderTexture;

    public RawImage rawImage;

    private void OnEnable()
    {
        if (redShader == null)
        {
            Debug.LogError("Missing shader");
            enabled = false;
            return;
        }

        redMaterial = new Material(redShader);
        commandBuffer = new CommandBuffer { name = "RedShaderEffect" };

        renderTexture = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24);
        // rawImage.texture = renderTexture;

        RenderPipelineManager.endCameraRendering += BeginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= BeginCameraRendering;

        if (redMaterial != null)
        {
            DestroyImmediate(redMaterial);
        }

        if (commandBuffer != null)
        {
            commandBuffer.Dispose();
        }
    }

    private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == cam)
        {
            Debug.Log("Rendering");
            commandBuffer.Clear();
            commandBuffer.Blit(camera.targetTexture, renderTexture, redMaterial);
            commandBuffer.Blit(renderTexture, camera.targetTexture);
            context.ExecuteCommandBuffer(commandBuffer);
        }
    }
}