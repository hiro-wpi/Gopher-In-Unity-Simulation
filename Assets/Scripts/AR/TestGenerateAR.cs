using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


// Will be used to test the GenerateARGameObject class
public class TestGenerateAR : MonoBehaviour
{
    public GameObject go;
    public GameObject ARFeature;
    public Vector3 positionOffset = new Vector3(0, 1, 0);
    public Vector3 rotationOffset = new Vector3(45, 0, 0);
    public Vector3 scale = new Vector3(1, 1, 1);
    public Color color = Color.red;

    // Start is called before the first frame update
    void Start()
    {
        
        if(go != null)
        {
            Debug.Log("Generating AR GameObject...");
            // ChangeMaterial();
            GenerateARGameObject.Generate(go, "cube", positionOffset, rotationOffset, scale, color, 0.25f);
            ARFeature = GenerateARGameObject.Generate(go, "sphere", positionOffset, rotationOffset, scale, color, 0.25f);
        }
        else
        {
            Debug.Log("GameObject is null");
        }
        
    }
}
