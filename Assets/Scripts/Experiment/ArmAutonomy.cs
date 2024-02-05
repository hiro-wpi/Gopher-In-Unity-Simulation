using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ArmAutonomy : MonoBehaviour
{
    [SerializeField] private GenerateARGameObject arGenerator;
    private GameObject armWaypointParent;
    [SerializeField] private ObjectSelector objectSelector;
    [SerializeField] private ArticulationArmController armController;
    [SerializeField] private AutoGrasping autoGrasping;
    private GameObject robot;
    [SerializeField] private GraphicalInterface graphicalInterface;
    [SerializeField] private RectTransform displayRect;
    private Camera cam;
    [SerializeField] private HighlightObjectOnCanvas highlightObject;
    [SerializeField, ReadOnly] private GameObject selectedObject;

    // [SerializeField] private Transform hoverTransform;
    // [SerializeField] private Transform graspTransform;

    void Start()
    {
        armWaypointParent = new GameObject("Arm Waypoints");
        armWaypointParent.transform.SetParent(transform);
        armWaypointParent.transform.localPosition = Vector3.zero;
        armWaypointParent.transform.localRotation = Quaternion.identity;
    }

    void OnEnable()
    {
        // Subscribe to the event
        objectSelector.OnObjectSelected += OnObjectSelected;

        // if (armController != null)
        // {
        //     armController.OnAutonomyComplete += OnGraspAutonomyComplete;
        //     armController.OnAutonomyComplete += OnHoverAutonomyComplete;
        // }
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up
        objectSelector.OnObjectSelected -= OnObjectSelected;

        // if (armController != null)
        // {
        //     armController.OnAutonomyComplete -= OnGraspAutonomyComplete;
        //     armController.OnAutonomyComplete -= OnHoverAutonomyComplete;
        //     // armController.OnAutonomyTrajectory -= OnArmTrajectoryGenerated;
        // }
    }

    // private void OnHoverAutonomyComplete()
    // {
    //     Debug.Log("Hover Autonomy Complete");
    //     armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
    // }

    // private void OnGraspAutonomyComplete()
    // {
    //     Debug.Log("Grasp Autonomy Complete");
    //     armController.SetAutonomyTarget(graspTransform.position, graspTransform.rotation);
    // }

    private void OnObjectSelected(GameObject gameObject, Vector3 position)
    {
        Debug.Log("Entered OnObjectSelected");

        if (robot == null || gameObject == null)
        {
            Debug.Log("Exiting OnObjectSelected");
            return;
        }

        // Check if there was a previously selected object
        if (selectedObject != null)
        {
            // Perform actions to cancel the previous selection
            autoGrasping.CancelCurrentTargetObject();
            highlightObject.RemoveHighlight(selectedObject);
        }

        selectedObject = gameObject;
        Debug.Log("Found selected object");
        
        highlightObject.Highlight(selectedObject, cam, displayRect);
        Debug.Log("Highlighted selected object");
        
        armController.SetGripperPosition(0.0f);
        Debug.Log("Opened gripper");
        
        var (hoverTransform, graspTransform) = autoGrasping.GetHoverAndGraspTransforms(selectedObject);
        Debug.Log("Retrieved hover transform");
        
        // armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
        // Debug.Log("Approached hover transform");
        
        // armController.SetAutonomyTarget(graspTransform.position, graspTransform.rotation);
        // armController.SetGripperPosition(1.0f);
        // armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
    }

    // private void OnArmTrajectoryGenerated()
    // {
    //     var (time, angles, velocities, accelerations) = 
    //         armController.GetAutonomyTrajectory();

    //     // Clear old waypoints
    //     foreach (Transform child in armWaypointParent.transform)
    //     {
    //         arGenerator.Destroy(child.gameObject);
    //         Destroy(child.gameObject);
    //     }

    //     // Add new waypoints
    //     foreach (var angle in angles)
    //     {
    //         GameObject waypoint = new GameObject("Waypoint");
    //         waypoint.transform.SetParent(armWaypointParent.transform);

    //         (waypoint.transform.position, waypoint.transform.rotation) =
    //             armController.GetEETargetPose(angle);

    //         arGenerator.Instantiate(
    //             waypoint,
    //             GenerateARGameObject.ARObjectType.Cube,
    //             scale: Vector3.one * 0.05f
    //         );
    //     }
    // }

    void Update()
    {
        if (robot == null)
        {
            robot = GameObject.Find("Gopher(Clone)");
            if (robot != null)
            {
                Transform rightArmHardware = robot.transform.Find("Plugins/Hardware/Right Arm");
                Transform rightArmAutonomy = robot.transform.Find("Plugins/Autonomy/Unity/Right Arm");

                armController = rightArmHardware.GetComponentInChildren<ArticulationArmController>();
                autoGrasping = rightArmAutonomy.GetComponentInChildren<AutoGrasping>();

                // armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
            }
        }

        // Get the main camera if it is not already set
        if (cam == null)
        {
            // Get all the active cameras referanced in the graphical interface
            Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();
            if (cameras.Length > 0)
            {
                cam = cameras[0];

                objectSelector.SetCameraAndDisplay(cam, displayRect);
            }
        }

        // Keyboard press space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            highlightObject.RemoveHighlight(selectedObject);
            autoGrasping.CancelCurrentTargetObject();
        } 
    }
}
