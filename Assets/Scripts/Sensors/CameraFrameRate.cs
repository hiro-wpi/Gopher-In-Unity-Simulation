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
    public Camera cam;
    public int targetFrameRate = 30;
    public bool rendering;

    void Start()
    {
        cam.enabled = false;
        // new enabled flag
        rendering = true;

        // Start rendering
        InvokeRepeating("Render", 0f, 1f / targetFrameRate);
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }
    void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        CancelInvoke("Render");
    }
    
    void Render()
    {
        if (rendering)
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
        // only work with target Texture but not target Display
        if (cam.targetTexture != null)
            cam.enabled = false;
    }

    public void SetFrameRate(int frameRate)
    {
        targetFrameRate = frameRate;
        CancelInvoke("Render");
        InvokeRepeating("Render", 0f, 1f / targetFrameRate);
    }
}
