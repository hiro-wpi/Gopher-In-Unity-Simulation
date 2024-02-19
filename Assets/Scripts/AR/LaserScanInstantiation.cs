using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///    Instantiate the laser scan results in the scene
/// </summary>
public class LaserScanInstantiation : MonoBehaviour
{
    [SerializeField] private bool visualize = true;

    [SerializeField] private Laser laser;
    [SerializeField] private GameObject laserScanPrefab;

    private GameObject[] laserObjects;

    void Start()
    {
        var (updateRate, samples, angleMin, angleMax, rangeMin, rangeMax) =
            laser.GetLaserScanParameters();

        laserObjects = new GameObject[samples];
        for (int i = 0; i < samples; i++)
        {
            laserObjects[i] = Instantiate(laserScanPrefab);
            laserObjects[i].transform.SetParent(transform);
            laserObjects[i].transform.localPosition = Vector3.zero;
            laserObjects[i].transform.localRotation = Quaternion.identity;
            laserObjects[i].name = "LaserScan" + i;
        }
    }

    void Update()
    {
        if (laser == null || !visualize)
        {
            SetLaserScan(false);
        }
        SetLaserScan(true);

        UpdateLaserScan();
    }

    private void SetLaserScan(bool active)
    {
        foreach (var obj in laserObjects)
        {
            obj.SetActive(active);
        }
    }

    private void UpdateLaserScan()
    {
        var (detected, positions) = laser.GetObstaclePositions();
        for (int i = 0; i < detected.Length; i++)
        {
            if (detected[i])
            {
                laserObjects[i].transform.localPosition = positions[i];
                laserObjects[i].SetActive(true);
            }
            else
            {
                laserObjects[i].SetActive(false);
            }
        }
    }
}
