using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenerateARGameObject
{
    public static GameObject Generate(GameObject gameObject, string type, Vector3 scale, Color color)
    {
        

        // Create the game object based on the type
        GameObject go;
        if(type == "cube")
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // return go;
        }
        else if(type == "sphere")
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // return go;
        }
        else
        {
            return null;
        }

        // Set parent
        go.transform.parent = gameObject.transform;

        // Set the position and rotation
        go.transform.position = gameObject.transform.position;
        go.transform.rotation = gameObject.transform.rotation;
        go.transform.localScale = scale; // the size of the object relative to the parent
                                        // global scale of the object can't be changed

        // Disable the collider
        go.GetComponent<Collider>().enabled = false;
        
        // Change the color

        Renderer renderer = go.GetComponent<Renderer>();
        Material material = renderer.material;
        Color newColor = material.color;
        newColor.a = 0.1f;
        material.color = newColor;

        // // Create a new material instance to ensure it's not affecting other objects
        // Material material = new Material(renderer.material);

        // // Set the rendering mode to Transparent
        // material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        // material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // material.SetInt("_ZWrite", 0);
        // material.DisableKeyword("_ALPHATEST_ON");
        // material.EnableKeyword("_ALPHABLEND_ON");
        // material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        // material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // // Set the alpha value of the material
        // Color color = material.color;
        // color.a = 0.1f;
        // material.color = color;

        // Assign the modified material to the renderer
        renderer.material = material;

        // go.GetComponent<Renderer>().material.color = color;
        // Color newColor = go.GetComponent<Renderer>().material.color;
        // newColor.a = 0.1f;

        // change the material to be transparent
        // go.GetComponent<Renderer>().material.SetFloat("_Mode", 2);

        // go.GetComponent<Renderer>().material.color = newColor;

        return go;
    }
}
