using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
///     This script can render camera at a target framerate
///     (if reachable).
///     Camera needs to be used with Rendertexture.
/// </summary>
public class CameraFrameRate : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private int targetFrameRate = 30;
    [field:SerializeField] public bool Rendering { get; set; }

    void Start()
    {
        if (cam != null)
        {
            cam.enabled = false;
        }
        
        // Use a new flag to control rendering
        Rendering = true;

        // Start rendering
        InvokeRepeating("Render", 0f, 1f / targetFrameRate);
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        CancelInvoke("Render");
    }
    
    private void Render()
    {
        if (cam == null)
            return;
        
        // Camera gets disabled after each rendering
        // Enable camera if Rendering set to true
        if (Rendering)
        {
            // this will cause problems for rendering dynamic objects
            // cam.Render();
            cam.enabled = true;
        }
        else
        {
            if (cam.enabled)
                cam.enabled = false;
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (cam == null)
            return;
        
        // Diable camera after rendering
        // only work with target Texture but not target Display
        if (cam.targetTexture != null)
            cam.enabled = false;
    }

    public void SetCamera(Camera camera)
    {
        cam = camera;
    }

    public int GetFrameRate()
    {
        return targetFrameRate;
    }

    public void SetFrameRate(int frameRate)
    {
        targetFrameRate = frameRate;
        // Relaunch rendering
        CancelInvoke("Render");
        InvokeRepeating("Render", 0f, 1f / targetFrameRate);
    }
}
