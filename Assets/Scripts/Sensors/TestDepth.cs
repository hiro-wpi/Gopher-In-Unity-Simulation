using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TestDepth : MonoBehaviour
{
    public Camera cam;
    public Shader redShader;
    private Material redMaterial;
    private CommandBuffer commandBuffer;

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

        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;

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
            commandBuffer.Clear();
            commandBuffer.Blit(camera.targetTexture, camera.targetTexture, redMaterial);
            context.ExecuteCommandBuffer(commandBuffer);
        }
    }
}