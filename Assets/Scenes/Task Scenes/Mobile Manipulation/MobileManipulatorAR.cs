using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MobileManipulatorAR : MonoBehaviour
{
    [Header("Robot")]
    [SerializeField] private GameObject robot;
    [SerializeField] private GameObject leftArmRoot;
    [SerializeField] private GameObject rightArmRoot;
    [SerializeField] private GameObject leftArmGripper;
    [SerializeField] private GameObject rightArmGripper;
    [SerializeField] private ArticulationEndEffectorController leftController;
    [SerializeField] private ArticulationEndEffectorController rightController;

    [Header("AR Rechability")]
    [SerializeField] private GenerateARGameObject arGenerator;
    [SerializeField] private DrawWaypoints drawWaypoints;
    [SerializeField] private GameObject gripper;

    // Gripper motion tracking
    private GameObject leftGripperMapping;
    private GameObject rightGripperMapping;

    // Reachability
    [SerializeField] private GameObject reachabilityPrefab;
    private GameObject leftReachability;
    private GameObject rightReachability;
    // Depth perception
    [SerializeField] private GameObject depthPrefab;
    private GameObject leftDepth;
    private GameObject rightDepth;

    [SerializeField] InputActionProperty inputAction;
    private GameObject[] arReachability;
    private bool arReachabilityEnabled = true;

    // Obstacles
    [Header("AR Obstacles")]
    private Camera cam;
    private RenderTexture rendererTexture;
    [SerializeField] private Laser laser;
    [SerializeField] private GameObject canvas;
    private GameObject obstacleDisplay;
    private bool obstacleDisplayEnabled = false;
    private Coroutine obstacleStopCoroutine;

    [Header("Task")]
    [SerializeField] private GameObject goal;
    [SerializeField] private AutoNavigation autoNavigation;
    private float replanTime = 1;
    private float timer = 0;

    [SerializeField] private Camera[] cameras;
    [SerializeField] private GameObject mapCover;
    private int interfaceIndex = 0;

    void Start()
    {
        // Obstacle camera
        // Camera
        GameObject obstacleCamera = new GameObject("ObstacleCamera");
        obstacleCamera.transform.parent = robot.transform;
        obstacleCamera.transform.localPosition = new Vector3(0, 2.5f, 0);
        obstacleCamera.transform.localRotation = Quaternion.Euler(90, 0, 0);
        cam = obstacleCamera.AddComponent<Camera>();
        cam.cullingMask = LayerMask.GetMask("Robot", "ARObject");
        cam.orthographic = true;
        cam.orthographicSize = 2.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.6f);
        rendererTexture = new RenderTexture(512, 512, 24);
        cam.targetTexture = rendererTexture;

        // RectTransform Canvas
        obstacleDisplay = new GameObject("ObstacleDisplay");
        obstacleDisplay.transform.parent = canvas.transform;
        RectTransform rect = obstacleDisplay.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420, 420);
        rect.anchoredPosition = new Vector2(-230, 60);
        RawImage image = obstacleDisplay.AddComponent<RawImage>();
        image.texture = rendererTexture;
        obstacleDisplay.SetActive(false);

        // Instantiate two game objects for motion tracking
        leftGripperMapping = new GameObject("MotionMappingLeft");
        rightGripperMapping = new GameObject("MotionMappingRight");
        leftGripperMapping.transform.parent = transform;
        rightGripperMapping.transform.parent = transform;
        arGenerator.Instantiate(leftGripperMapping, gripper);
        arGenerator.Instantiate(rightGripperMapping, gripper);

        // Instantiate four game objects for reachability and depth perception
        leftReachability = Instantiate(reachabilityPrefab);
        rightReachability = Instantiate(reachabilityPrefab);
        leftDepth = Instantiate(depthPrefab);
        rightDepth = Instantiate(depthPrefab);
        leftReachability.transform.parent = transform;
        rightReachability.transform.parent = transform;
        leftDepth.transform.parent = transform;
        rightDepth.transform.parent = transform;

        // Visibility
        // inputAction.action.performed += ctx => OnChangeVisibility(ctx);
        arReachability = new GameObject[4] {
            leftReachability, rightReachability, leftDepth, rightDepth
        };
    }

    void Update()
    {
        // Check if the obstacle map is needed
        if (CheckObstacleDistance())
        {
            obstacleDisplay.SetActive(true);
            obstacleDisplayEnabled = true;
        }
        else
        {
            if (obstacleDisplayEnabled)
            {
                if (obstacleStopCoroutine != null)
                {
                    StopCoroutine(obstacleStopCoroutine);
                }
                obstacleStopCoroutine = StartCoroutine(
                    DisableObstacleDisplay(1.0f)
                );

                obstacleDisplayEnabled = false;
            }
        }

        // Update leftGripper and rightGripper
        // leftGripper.transform.position = leftController.outPosition;
        // leftGripper.transform.rotation = leftController.outRotation;
        // rightGripper.transform.position = rightController.outPosition;
        // rightGripper.transform.rotation = rightController.outRotation;

        // Update reachability
        if (arReachabilityEnabled)
        {
            // Rechability
            leftReachability.transform.parent = transform;
            leftReachability.transform.position = new Vector3(
                leftArmRoot.transform.position.x,
                0.0f,
                leftArmRoot.transform.position.z
            );
            rightReachability.transform.parent = transform;
            rightReachability.transform.position = new Vector3(
                rightArmRoot.transform.position.x,
                0.0f,
                rightArmRoot.transform.position.z
            );

            // Depth
            leftDepth.transform.position = leftArmGripper.transform.position;
            leftDepth.transform.rotation = leftArmGripper.transform.rotation;
            rightDepth.transform.position = rightArmGripper.transform.position;
            rightDepth.transform.rotation = rightArmGripper.transform.rotation;
        }

        // Check replan
        timer += Time.deltaTime;
        if (timer > replanTime)
        {
            timer = 0;
            autoNavigation.SetGoal(
                goal.transform.position,
                goal.transform.rotation,
                trajectoryReceived
            );
        }

        // Space key pressed
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnChangeVisibility();
        }

        // Space key pressed
        if (Input.GetKeyDown(KeyCode.Return))
        {
            OnToggleInterface();
        }
    }

    private IEnumerator DisableObstacleDisplay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        obstacleDisplay.SetActive(false);
    }

    private void trajectoryReceived(Vector3[] waypoints)
    {
        drawWaypoints.DrawLine(
            "mm-trajectory",
            waypoints
        );
    }

    private bool CheckObstacleDistance()
    {
        bool leftClose = false;
        bool rightClose = false;
        for (int i = 0; i < laser.Directions.Length; ++i)
        {
            if (
                laser.Directions[i] < -0.7854f
                && laser.ObstacleRanges[i] < 0.8f
            )
            {
                leftClose = true;
            }

            if (
                laser.Directions[i] > 0.7854f
                && laser.ObstacleRanges[i] < 0.8f
            )
            {
                rightClose = true;
            }
        }

        return leftClose && rightClose;
    }

    private void OnChangeVisibility()
    {
        arReachabilityEnabled = !arReachabilityEnabled;
        foreach (GameObject obj in arReachability)
        {
            obj.SetActive(arReachabilityEnabled);
        }
    }

    private void OnToggleInterface()
    {
        interfaceIndex = (interfaceIndex + 1) % 3;

        if (interfaceIndex == 0 || interfaceIndex == 2)
        {
            foreach (Camera camera in cameras)
            {
                // Enbale ARObject to render
                int layer = LayerMask.NameToLayer("ARObject");
                camera.cullingMask |= 1 << layer;
            }
        }
        else
        {
            foreach (Camera camera in cameras)
            {
                // Remove ARObject from render
                int layer = LayerMask.NameToLayer("ARObject");
                camera.cullingMask &= ~(1 << layer);
            }
        }

        if (interfaceIndex == 1 || interfaceIndex == 2)
        {
            mapCover.SetActive(false);
        }
        else
        {
            mapCover.SetActive(true);
        }
    }
}
