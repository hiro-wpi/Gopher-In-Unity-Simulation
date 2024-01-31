using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


public static class GenerateARGameObject
{
    public static GameObject Generate(GameObject gameObject, string type, Vector3 scale, Color color, float transparency = 0.25f)
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

        ChangeMaterial(go, color, transparency);

        return go;
    }

    // Change the material of the gameobject
    private static void ChangeMaterial(GameObject obj, Color color, float transparency)
    {
        
        Renderer renderer = obj.GetComponent<Renderer>();

        // Create a new material instance to ensure it's not affecting other objects
        Material transparentMaterial = loadTransparentMaterial();
        Material material = new Material(transparentMaterial);

        // Create a transparent red
        Color newColor = color;
        newColor.a = transparency;

        // Set the color
        material.color = newColor;
        renderer.material = material;

    }

    // Loads the transparent material from the path
    private static Material loadTransparentMaterial()
    {
        string materialPath = "Assets/Materials/General/TransparentWhite.mat"; // Path to your material
        
        // Load the material using AssetDatabase
        Material loadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (loadedMaterial != null)
        {
            // Use the loaded material
            Debug.Log("Material loaded successfully!");
            return loadedMaterial;
        }
        else
        {
            // The material was not found
            Debug.LogError("Failed to load material at path: " + materialPath);
            return new Material(Shader.Find("Standard"));
        }
    }
}
