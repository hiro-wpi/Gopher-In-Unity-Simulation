using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Define a RGB camera sensor by setting a tag and rendertexture.
///   
///     It is not always necessary as
///     Tag and RenderTexture can also be set outside
/// </summary>
[RequireComponent(typeof(Camera))]
public class RGBCamera : MonoBehaviour
{
    // Camera
    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();

        // Set this tag to "DepthCamera" in Unity Editor
        // As only camera with this tag will be rendered with
        // DepthShader.shader (written in DepthRendererFeature)
        tag = "RGBCamera";

        // Use a higher precision 1-channel render texture for depth camera
        cam.targetTexture = new RenderTexture(
            cam.pixelWidth, cam.pixelHeight, 24, RenderTextureFormat.ARGB32
        );
    }

    void Update() {}
}