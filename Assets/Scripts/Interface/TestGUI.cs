using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestGUI : MonoBehaviour
{
    // Main UI
    public GameObject cameraDisplay;
    private Vector2 cameraResolution;
    private RectTransform cameraDisplayRect;
    // Map
    private RenderTexture mapRendertexture;
    private GameObject mapCameraObject;
    private Camera mapCamera;
    private Vector3 prevClickPoint = Vector3.zero;

    // Robot
    public GameObject robot;
    public ROSAutoNavigation autoNavigation;
    
    // Path visualization
    /*
    public GameObject goalPrefab;
    private GameObject goalObject;
    */
    public LineRenderer localLineRenderer;
    public LineRenderer globalLineRenderer;

    void Start() 
    {
        // planner
        autoNavigation = robot.GetComponentInChildren<ROSAutoNavigation>();

        // map render texture
        cameraDisplayRect = cameraDisplay.GetComponent<RectTransform>();
        cameraResolution = new Vector2(
            (int)cameraDisplayRect.rect.width, 
            (int)cameraDisplayRect.rect.height
        );
        mapRendertexture = new RenderTexture(
            (int)cameraResolution.x, (int)cameraResolution.y, 24
        );
        cameraDisplay.GetComponent<RawImage>().texture = mapRendertexture;

        // map camera game object
        if (mapCameraObject != null)
            Destroy(mapCameraObject);
        mapCameraObject = new GameObject("Map Camera");
        mapCameraObject.transform.parent = transform;
        mapCameraObject.transform.position = new Vector3(0f, 5f, 0f);
        mapCameraObject.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));

        // map camera setting
        mapCamera = mapCameraObject.AddComponent<Camera>();
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = 14f;
        mapCamera.cullingMask = LayerMask.GetMask("Robot", "Map", "UI");
        mapCamera.targetTexture = mapRendertexture;
    }

    void Update()
    {
        if (autoNavigation == null)
            return;

        // Check key input
        if (Input.GetKeyDown(KeyCode.G))
            autoNavigation.ResumeNavigation();
        if (Input.GetKeyDown(KeyCode.H))
            autoNavigation.PauseNavigation();
        if (Input.GetKeyDown(KeyCode.T))
            autoNavigation.StopNavigation();

        // Set goal
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mapCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                SetNavigationGoal(hit.point);
        }

        // Draw path
        DrawPath(autoNavigation.LocalWaypoints, localLineRenderer);
        DrawPath(autoNavigation.GlobalWaypoints, globalLineRenderer);
    }

    private void SetNavigationGoal(Vector3 point)
    {
        // Cancel previous goal
        if ((point - prevClickPoint).magnitude < 0.5)
        {
            autoNavigation.StopNavigation();
            prevClickPoint = Vector3.zero;
        }
        
        // Set goal
        else
        {
            autoNavigation.SetGoal(point);
            prevClickPoint = point;
        }
    }

    private void DrawPath(
        Vector3[] waypoints, LineRenderer lineRenderer, bool global = true)
    {
        // If no path is available, do not draw
        if (waypoints.Length == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        // Draw current point + waypoints
        lineRenderer.positionCount = (1 + waypoints.Length);
        lineRenderer.SetPosition(0, robot.transform.position);
        for (int i = 0; i < waypoints.Length; ++i)
        {
            // y higher for better visualization
            lineRenderer.SetPosition(
                1 + i, waypoints[i] + new Vector3(0f, 0.05f, 0f)
            ); 
        }
        
        // Draw goal
        /*
        if (goalObject != null && 
            goalObject.transform.position != waypoints[^1])
        {
            Debug.Log(goalObject.transform.position);
            Debug.Log(waypoints[^1]);
            Debug.Log("Destroy");
            Destroy(goalObject);
        }
        if (goalObject == null)
        {
            Debug.Log("Goal");
            goalObject = Instantiate(
                goalPrefab, 
                waypoints[^1],
                Quaternion.identity
            );
            Utils.SetGameObjectLayer(goalObject, "Robot", true);
        }
        */
    } 
}
