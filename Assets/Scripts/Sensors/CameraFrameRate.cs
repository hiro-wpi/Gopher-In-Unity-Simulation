using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
///     This script can render camera at a target framerate
///     (if reachable), by enable/disable camera.
///
///     Camera needs to be used with Rendertexture.
/// </summary>
public class CameraFrameRate : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private int targetFrameRate = 30;
    private Timer timer = new Timer(30);

    [field:SerializeField] public bool Rendering { get; set; }

    void Start()
    {
        cam.enabled = false;
        // Use a new flag to control rendering
        Rendering = true;

        // Rate
        timer = new Timer(targetFrameRate);

        // Start rendering
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDestroy()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
    }

    void Update()
    {
        timer.UpdateTimer(Time.deltaTime);
        // Enable camera if time is up
        if (timer.ShouldProcess)
        {
            Render();
            timer.ShouldProcess = false;
        }
    }
    
    private void Render()
    {
        if (cam == null)
        {
            return;
        }
        
        // Camera gets disabled after each rendering
        // Enable camera if Rendering set to true
        if (Rendering)
        {
            // Calling cam.Render() will cause problems 
            // for rendering dynamic objects
            cam.enabled = true;
        }
        else
        {
            cam.enabled = false;
        }
    }

    void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (cam == null)
        {
            return;
        }
        
        // Diable camera after rendering
        // only work with target Texture but not target Display
        if (cam.targetTexture != null)
        {
            cam.enabled = false;
        }
    }

    public void SetCamera(Camera camera)
    {
        cam = camera;
    }

    public void SetFrameRate(int frameRate)
    {
        targetFrameRate = frameRate;
        timer = new Timer(targetFrameRate);
    }

    public int GetFrameRate()
    {
        return targetFrameRate;
    }
}
