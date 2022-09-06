using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///    This script simulates a laser scanner to
///    detect the surrounding obstacles.
///
///    Inherited from SurroundingDetection
///    Most methods are the same, but one more method 
///    is written to instantiate numerous spheres to represent scan result
/// </summary>
public class Laser : SurroundingDetection
{
    public bool instantiateScanResult;
    public GameObject scanResultPrefab;
    private GameObject scanResultParent;
    private GameObject[] scanResultObjects;

    new void Start()
    {
        base.Start();

        // Visualize it in the scene
        if (instantiateScanResult)
        {
            InstantiateScanResultObjects();
            InvokeRepeating("UpdateScanResult", 1f, 1f / updateRate);
        }
    }

    private void UpdateScanResult()
    {
        // Cast rays towards diffent directions to find colliders
        for (int i = 0; i < samples; ++i)
        {
            // Not detected
            if (obstacleRanges[i] == 0f)
            {
                scanResultObjects[i].SetActive(false);
            }   
            // Detected
            else
            {
                scanResultObjects[i].SetActive(true);
                // Angle
                Vector3 rotation = rayRotations[i] * rayStartForward;
                scanResultObjects[i].transform.position = 
                    rayStartPosition + obstacleRanges[i] * rotation;
            }
        }
    }

    private void InstantiateScanResultObjects()
    {
        // Instantiate game objects for visualization  
        scanResultParent = new GameObject("Scan Result Spheres");
        scanResultObjects = new GameObject[samples];
        for (int i = 0; i < samples; ++i)
        {
            scanResultObjects[i] = Instantiate(scanResultPrefab, 
                                               Vector3.zero, 
                                               Quaternion.identity);
            scanResultObjects[i].layer = LayerMask.NameToLayer("Laser");
            scanResultObjects[i].transform.parent = scanResultParent.transform;
        }
    }
}
