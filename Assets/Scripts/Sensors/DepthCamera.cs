using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Unity.Collections;

/// <summary>
///     This script enable the Unity camera to 
///     render a depth image
/// </summary>
public class DepthCamera : MonoBehaviour
{
    public Shader depthShader;
    private Camera cam;
    public RenderTexture tempRT;
    private Material depthMaterial;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Create a temporary RenderTexture of the same size as the camera's output
        tempRT = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24);
        
        // Create a new Material with the depth shader
        depthMaterial = new Material(depthShader);

        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera != cam)
            return;
        
        // Blit the camera's RenderTexture to the temporary one, applying the depth shader
        Graphics.Blit(cam.targetTexture, tempRT, depthMaterial);
        
        // Blit the temporary RenderTexture back to the camera's one
        Graphics.Blit(tempRT, cam.targetTexture);
    }
}
