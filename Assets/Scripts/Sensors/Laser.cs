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
    private bool instantiateScanResult;
    public GameObject scanResultPrefab;
    private GameObject scanResultParent;
    private GameObject[] scanResultObjects;


    void OnDestroy() 
    {
        SetScanResultInstantiationActive(false);
    }

    public void SetScanResultInstantiationActive(bool active)
    {
        if (instantiateScanResult == active)
            return;
        
        instantiateScanResult = active;
        if (active)
        {
            InstantiateScanResultObjects();
            InvokeRepeating("UpdateScanResult", 1f, 1f / updateRate);
        }
        else
        {
            CancelInvoke("UpdateScanResult");
            Destroy(scanResultParent);
        }
    }

    private void UpdateScanResult()
    {
        // Cast rays towards diffent directions to find colliders
        for (int i = 0; i < samples; ++i)
        {
            // Angle
            Vector3 rotation = rayRotations[i] * rayStartForward;
            scanResultObjects[i].transform.position = 
                rayStartPosition + obstacleRanges[i] * rotation;
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
