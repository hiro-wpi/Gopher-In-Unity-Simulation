using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Define a depth camera sensor by setting a tag and rendertexture.
//      This script is used in Universal Render Pipeline with 
///     DepthShader.shader and DepthRendererFeature
///   
///     It is not always necessary as
///     Tag and RenderTexture can also be set outside
/// </summary>
[RequireComponent(typeof(Camera))]
public class DepthCamera : MonoBehaviour
{
    // Camera
    private Camera cam;
    [SerializeField] private int resolutionWidth = 1280;
    [SerializeField] private int resolutionHeight = 720;
    
    void Awake()
    {
        cam = GetComponent<Camera>();

        // Set this tag to "DepthCamera" in Unity Editor
        // As only camera with this tag will be rendered with
        // DepthShader.shader (written in DepthRendererFeature)
        tag = "DepthCamera";
        
        // Use a higher precision 1-channel render texture for depth camera
        cam.targetTexture = new RenderTexture(
            resolutionWidth, resolutionHeight, 32, RenderTextureFormat.RFloat
        );
    }

    void Start() {}

    void Update() {}
}