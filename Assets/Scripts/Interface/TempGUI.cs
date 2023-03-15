using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Unity.Robotics.Visualizations;


public class TempGUI : MonoBehaviour
{
    // Camera source
    [SerializeField] private CompressedImageSubscriber mainCameraSubscriber;
    [SerializeField] private CompressedImageSubscriber secondaryCameraSubscriber;
    // Laser scan source
    [SerializeField] private LaserScanSubscriber laserScanSubscriber;
    // Robot model
    [SerializeField] private GameObject robotModelPrefab;
    private GameObject robotModelGO;

    // Main camera
    [SerializeField] private GameObject cameraDisplay;
    private Vector2 cameraResolution;
    private RectTransform cameraDisplayRect;
    private RenderTexture cameraRendertexture;
    // Secondary camera
    [SerializeField] private GameObject secondayCameraDisplay;
    private RectTransform secondayCameraDisplayRect;
    private RenderTexture secondayCameraRendertexture;
    // Map
    [SerializeField] private GameObject mapDisplay;
    private RectTransform mapDisplayRect;
    private RenderTexture mapRendertexture;
    private GameObject mapCameraGO;
    private Camera mapCamera;
    // Point cloud
    [SerializeField] private GameObject pointCloudPrefab;
    private GameObject pointCloudGO;
    private GameObject[] pointCloudObjects;
    PointCloudDrawing pointCloud;

    void Start()
    {
        // main render texture
        cameraDisplayRect = cameraDisplay.GetComponent<RectTransform>();
        cameraRendertexture = new RenderTexture((int)cameraDisplayRect.rect.width, 
                                                (int)cameraDisplayRect.rect.height, 24);
        cameraDisplay.GetComponent<RawImage>().texture = cameraRendertexture;

        // secondary render texture
        secondayCameraDisplayRect = secondayCameraDisplay.GetComponent<RectTransform>();
        secondayCameraRendertexture = new RenderTexture((int)secondayCameraDisplayRect.rect.width, 
                                                        (int)secondayCameraDisplayRect.rect.height, 24);
        secondayCameraDisplay.GetComponent<RawImage>().texture = secondayCameraRendertexture;

        // subscribe to camera source
        mainCameraSubscriber.TargetTexture = cameraRendertexture;
        secondaryCameraSubscriber.TargetTexture = secondayCameraRendertexture;

        // map render texture
        mapDisplayRect = mapDisplay.GetComponent<RectTransform>();
        mapRendertexture = new RenderTexture((int)mapDisplayRect.rect.width, 
                                             (int)mapDisplayRect.rect.height, 24);
        mapDisplay.GetComponent<RawImage>().texture = mapRendertexture;
        // map camera
        SetupMapCamera();

        // laser scan
        // TODO
        // robot model
        robotModelGO = Instantiate(robotModelPrefab, Vector3.zero, Quaternion.identity);
    }

    void Update()
    { 
        // Draw scan data
        if (pointCloudGO == null)
        {
            InstantiateScanResultObjects(laserScanSubscriber.Angles, laserScanSubscriber.Ranges);
        }
        else
        {
            UpdateScanResult(laserScanSubscriber.Angles, laserScanSubscriber.Ranges);
        }    
    }

    private void SetupMapCamera()
    {
        // generate game object
        mapCameraGO = new GameObject("map Camera");
        mapCameraGO.transform.parent = null;
        mapCameraGO.transform.localPosition = new Vector3(0f, 5f, 1f);
        mapCameraGO.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
        // map camera settings
        mapCamera = mapCameraGO.AddComponent<Camera>();
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = 2.0f;
        // map rendering settings
        mapCamera.targetTexture = mapRendertexture;
    }

    /*
    public void DrawLaserScan(float[] angles, float[] ranges)
    {
        // Check if the data is valid
        if (angles.Length == 0 || ranges.Length == 0)
        {
            return;
        }

        // Create point cloud drawer
        pointCloud = PointCloudDrawing.Create(
            pointCloudGO, angles.Length, null
        );
        
        // Draw each point
        for (int i = 0; i < angles.Length; i++)
        {
            // scan 3d position
            Vector3 point = Quaternion.Euler(0, Mathf.Rad2Deg * angles[i], 0) 
                          * Vector3.forward * ranges[i];
            // scan color
            Color32 color = Color.HSVToRGB(
                Mathf.InverseLerp(0.0f, 5.0f, ranges[i]), 1, 1
            );
            // scan radius
            float radius = 0.05f;

            pointCloud.AddPoint(point, color, radius);
        }
        pointCloud.Bake();
    }
    */
    private void UpdateScanResult(float[] angles, float[] ranges)
    {
        // Cast rays towards diffent directions to find colliders
        for (int i = 0; i < angles.Length; ++i)
        {
            // Not detected, not a number, or too large
            if (ranges[i] == 0f || float.IsNaN(ranges[i]) || ranges[i] > 10f)
            {
                pointCloudObjects[i].SetActive(false);
            }   
            // Detected
            else
            {
                pointCloudObjects[i].SetActive(true);
                // Angle
                Vector3 point = Quaternion.Euler(0, Mathf.Rad2Deg * angles[i], 0) 
                              * Vector3.forward * ranges[i];
                pointCloudObjects[i].transform.position = point;
            }
        }
    }

    private void InstantiateScanResultObjects(float[] angles, float[] ranges)
    {
        // Check if the data is valid
        if (angles.Length == 0 || ranges.Length == 0)
        {
            return;
        }

        // Instantiate game objects for visualization  
        pointCloudGO = new GameObject("Point Cloud");
        pointCloudObjects = new GameObject[angles.Length];
        for (int i = 0; i < angles.Length; ++i)
        {
            pointCloudObjects[i] = Instantiate(pointCloudPrefab, 
                                               Vector3.zero, 
                                               Quaternion.identity);
            pointCloudObjects[i].layer = LayerMask.NameToLayer("Laser");
            pointCloudObjects[i].transform.parent = pointCloudGO.transform;
        }
    }
}
