using System.Collections;
using System.Collections.Generic;
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
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up
        objectSelector.OnObjectSelected -= OnObjectSelected;

        // if (armController != null)
        // {
        //     armController.OnAutonomyTrajectory -= OnArmTrajectoryGenerated;
        // }
    }

    private void OnObjectSelected(GameObject gameObject, Vector3 position)
    {
        if (robot == null || gameObject == null)
        {
            return;
        }

        selectedObject = gameObject;
        highlightObject.Highlight(selectedObject, cam, displayRect);
        autoGrasping.SetTargetObject(selectedObject);
        armController.MoveToAutonomyTarget();
        armController.SetGripperPosition(0.0f);
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
                armController =
                    robot.GetComponentInChildren<ArticulationArmController>();
                autoGrasping = 
                    robot.GetComponentInChildren<AutoGrasping>();

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
            armController.CancelAutonomyTarget();
        } 
    }
}
