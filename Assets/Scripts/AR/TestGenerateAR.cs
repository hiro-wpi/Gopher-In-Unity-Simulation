using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


// Will be used to test the GenerateARGameObject class
public class TestGenerateAR : MonoBehaviour
{
    public GameObject go;
    public GameObject ARFeature;

    public string type = "cube";
    public Vector3 scale = new Vector3(1, 1, 1);
    public Color color = new Color(1f, 0.0f, 0.0f, 1f);

    // public Material transparentMaterial;



    // Start is called before the first frame update
    void Start()
    {
        
        if(go != null)
        {
            Debug.Log("Generating AR GameObject...");
            ChangeMaterial();
            // loadMaterial();
            // GenerateARGameObject.Generate(go, type, scale, color);
        }
        else
        {
            Debug.Log("GameObject is null");
        }
        
    }


    void ChangeMaterial()
    {
        
        Renderer renderer = go.GetComponent<Renderer>();

        // Create a new material instance to ensure it's not affecting other objects
        Material transparentMaterial = loadMaterial();
        Material material = new Material(transparentMaterial);

        // Create a transparent red
        Color newColor = Color.red;
        newColor.a = 0.1f;

        // Set the color
        material.color = newColor;
        renderer.material = material;   

        
    }

    Material loadMaterial()
    {
        string materialPath = "Assets/Materials/General/TransparentWhite.mat"; // Path to your material
        
        // Load the material using AssetDatabase
        Material loadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (loadedMaterial != null)
        {
            // Use the loaded material
            // GetComponent<Renderer>().material = loadedMaterial;
            Debug.Log("Material loaded successfully!");
            return loadedMaterial;
        }
        else
        {
            Debug.LogError("Failed to load material at path: " + materialPath);
            return null;
        }
    }

    

    // void Start()
    // {
    //     // Load the material using AssetDatabase
    //     Material loadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

    //     if (loadedMaterial != null)
    //     {
    //         // Use the loaded material
    //         GetComponent<Renderer>().material = loadedMaterial;
    //     }
    //     else
    //     {
    //         Debug.LogError("Failed to load material at path: " + materialPath);
    //     }
    // }
}
