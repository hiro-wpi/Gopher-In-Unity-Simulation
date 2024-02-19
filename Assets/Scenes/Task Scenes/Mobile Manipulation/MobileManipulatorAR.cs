using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MobileManipulatorAR : MonoBehaviour
{
    [Header("Robot")]
    [SerializeField] private GameObject leftArmRoot;
    [SerializeField] private GameObject rightArmRoot;
    [SerializeField] private GameObject leftArmGripper;
    [SerializeField] private GameObject rightArmGripper;
    [SerializeField] private ArticulationEndEffectorController leftController;
    [SerializeField] private ArticulationEndEffectorController rightController;

    [Header("AR")]
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

    private void trajectoryReceived(Vector3[] waypoints)
    {
        drawWaypoints.DrawLine(
            "mm-trajectory",
            waypoints
        );
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
