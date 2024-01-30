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
        go.transform.scale = size;

        // Disable the collider
        go.GetComponent<Collider>().enabled = false;
        
        // Change the color
        go.GetComponent<Renderer>().material.color = color;
        Color newColor = go.GetComponent<Renderer>().material.color;
        newColor.a = 0.5f;
        go.GetComponent<Renderer>().material.color = newColor;

        return go;
    }
}
