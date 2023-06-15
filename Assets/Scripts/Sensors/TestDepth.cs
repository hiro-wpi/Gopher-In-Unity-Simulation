using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class TestDepth : MonoBehaviour
{
    public Material depthDisplayMaterial;

    private void Start()
    {
        depthDisplayMaterial.SetTexture("_MainTex", Shader.GetGlobalTexture("_CameraDepthTexture"));
    }
}