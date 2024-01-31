using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeRenderingMode : MonoBehaviour
{
    // Enum to represent various rendering modes of the Standard Shader
    public GameObject obj;

    public enum StandardShaderRenderingMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent
    }

    void Start()
    {
        // Call the function to change rendering mode to transparent
        ChangeRenderingModeForGameObject(obj, StandardShaderRenderingMode.Transparent);
    }

    // Function to set the rendering mode of a material
    void ChangeRenderingModeForGameObject(GameObject obj, StandardShaderRenderingMode renderingMode)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = renderer.material;
            ChangeMaterialRenderingMode(material, renderingMode);
        }
        else
        {
            Debug.LogWarning("Renderer component not found on the GameObject: " + obj.name);
        }
    }

    // Function to set the rendering mode of a material
    void ChangeMaterialRenderingMode(Material material, StandardShaderRenderingMode renderingMode)
    {
        switch (renderingMode)
        {
            case StandardShaderRenderingMode.Opaque:
                material.SetFloat("_Mode", 0); // Opaque mode
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;

            case StandardShaderRenderingMode.Cutout:
                // Customize for Cutout if needed
                break;

            case StandardShaderRenderingMode.Fade:
                // Customize for Fade if needed
                break;

            case StandardShaderRenderingMode.Transparent:
                material.SetFloat("_Mode", 3); // Transparent mode
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
        }
    }
}
