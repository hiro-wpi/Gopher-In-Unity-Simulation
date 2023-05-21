using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// TODO
//      Find a better inplementation for changing the base footprint.
//      Used in Function Update()

public class TestGUI : MonoBehaviour
{
    // Main UI
    public GameObject cameraDisplay;
    public GameObject minimapCameraDisplay;
    private Vector2 cameraResolution;
    private Vector2 minimapCameraResolution;
    private RectTransform cameraDisplayRect;
    private RectTransform minimapCameraDisplayRect;
    // Map
    private RenderTexture mapRendertexture;
    private GameObject mapCameraObject;
    private Camera mapCamera;
    private Vector3 prevClickPoint = Vector3.zero;
    // Minimap
    private RenderTexture minimapRendertexture;
    private GameObject minimapCameraObject;
    private Camera minimapCamera;

    // Robot
    public GameObject robot;
    // public GameObject robotFollower;
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
        //      create a live feed of an image that has the same dimentions 
        //      as the rectangle game element
        cameraDisplayRect = cameraDisplay.GetComponent<RectTransform>();
        cameraResolution = new Vector2(
            (int)cameraDisplayRect.rect.width, 
            (int)cameraDisplayRect.rect.height
        );
        mapRendertexture = new RenderTexture(
            (int)cameraResolution.x, (int)cameraResolution.y, 24
        );
        cameraDisplay.GetComponent<RawImage>().texture = mapRendertexture;

        // map render texture
        //      create a live feed of an image that has the same dimentions 
        //      as the rectangle game element
        minimapCameraDisplayRect = minimapCameraDisplay.GetComponent<RectTransform>();
        minimapCameraResolution = new Vector2(
            (int)minimapCameraDisplayRect.rect.width, 
            (int)minimapCameraDisplayRect.rect.height
        );
        minimapRendertexture = new RenderTexture(
            (int)minimapCameraResolution.x, (int)minimapCameraResolution.y, 24
        );
        minimapCameraDisplay.GetComponent<RawImage>().texture = minimapRendertexture;

        // map camera game object
        //      create a camera game object (not the camera itself) 
        //      in the Unity world that point downward at a map
        if (mapCameraObject != null)
            Destroy(mapCameraObject);
        mapCameraObject = new GameObject("Map Camera");
        mapCameraObject.transform.parent = transform; // parent is the large canvas. The location of the tranfrom is at the corner of the canvas. 
        mapCameraObject.transform.position = new Vector3(0f, 5f, 0f);
        mapCameraObject.transform.rotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));

        // map camera setting
        mapCamera = mapCameraObject.AddComponent<Camera>();
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = 14f;
        mapCamera.cullingMask = LayerMask.GetMask("Robot", "Map", "UI");  
            // ^^ These are Layers defined in the Inspector Window In Unity. They are attached to
            //    Robot -> Gopher
            //    Map   -> Hospital Map Environement
            //    UI    -> Local and Global Paths
            //             There exist other UI elements, however, those are not directly below the camera

        mapCamera.targetTexture = mapRendertexture;


        // Lets make the camera minimap object
        if (minimapCameraObject != null)
            Destroy(minimapCameraObject);
        minimapCameraObject = new GameObject("Minimap Camera");
        minimapCameraObject.transform.parent = robot.transform; // parent is the large canvas. The location of the tranfrom is at the corner of the canvas. 
        minimapCameraObject.transform.position = new Vector3(robot.transform.position.x, 5f, robot.transform.position.z);
        minimapCameraObject.transform.rotation = Quaternion.Euler(new Vector3(90f, robot.transform.eulerAngles.y, robot.transform.eulerAngles.z));

        // map camera setting
        minimapCamera = minimapCameraObject.AddComponent<Camera>();
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = 2f;
        minimapCamera.cullingMask = LayerMask.GetMask("Robot", "Map", "UI");  
            // ^^ These are Layers defined in the Inspector Window In Unity. They are attached to
            //    Robot -> Gopher
            //    Map   -> Hospital Map Environement
            //    UI    -> Local and Global Paths
            //             There exist other UI elements, however, those are not directly below the camera

        minimapCamera.targetTexture = minimapRendertexture;

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

        // Changing Footprint
        if (Input.GetKeyDown(KeyCode.J))
            autoNavigation.SetToNormalFootprint();
        if (Input.GetKeyDown(KeyCode.K))
            autoNavigation.SetToBaseWithCartFootprint();

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
