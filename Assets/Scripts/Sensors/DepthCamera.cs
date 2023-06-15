using UnityEngine;

[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class DepthEffect : MonoBehaviour
{
    public Material DepthMaterial;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (DepthMaterial != null)
        {
            Graphics.Blit(source, destination, DepthMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}