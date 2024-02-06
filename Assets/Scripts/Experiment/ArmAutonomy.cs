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
    [SerializeField] private GameObject arGripper;
    [SerializeField] private GameObject selectedObject;
    [SerializeField] private Transform hoverTransform;
    [SerializeField] private Transform graspTransform;
    [SerializeField] private bool reachedGoal = false;
    private bool hasNewGoal = false;
    private GameObject goalObject;

    public bool gotTrajectory = false;

    private enum AutonomyState
    {
        SelectObject,
        FirstHoverOverObject,
        GraspObject,
        SecondHoverOverObject,
        DeliverToGoal
    }
    private AutonomyState currentState;

    void Start()
    {
        armWaypointParent = new GameObject("Arm Waypoints");
        armWaypointParent.transform.SetParent(transform);
        armWaypointParent.transform.localPosition = Vector3.zero;
        armWaypointParent.transform.localRotation = Quaternion.identity;

        currentState = AutonomyState.SelectObject;
    }

    void OnEnable()
    {
        // Subscribe to the event
        objectSelector.OnObjectSelected += OnObjectSelected;

        if (armController != null)
        {
            armController.OnAutonomyComplete += OnArmReachedGoal;
            armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up
        objectSelector.OnObjectSelected -= OnObjectSelected;

        if (armController != null)
        {
            armController.OnAutonomyComplete -= OnArmReachedGoal;
            armController.OnAutonomyTrajectory -= OnArmTrajectoryGenerated;
        }
    }

    // reachedGoal not being set to true <-- FIX
    private void OnArmReachedGoal()
    {
        Debug.Log("Arm Reached Goal");
        reachedGoal = true;
    }

    private void OnArmTrajectoryGenerated()
    {
        Debug.Log("We got a trajectory");
        gotTrajectory = true;
        var (time, angles, velocities, accelerations) = 
            armController.GetAutonomyTrajectory();

        foreach (Transform child in armWaypointParent.transform)
        {
            arGenerator.Destroy(child.gameObject);
            Destroy(child.gameObject);
        }

        foreach (var angle in angles)
        {
            GameObject waypoint = Instantiate(arGripper);
            waypoint.transform.SetParent(armWaypointParent.transform);

            (waypoint.transform.position, waypoint.transform.rotation) =
                armController.GetEETargetPose(angle);

            arGenerator.Instantiate(
                waypoint,
                arGripper
            );
        }

        armController.MoveToAutonomyTarget();
    }

    private void OnObjectSelected(GameObject gameObject, Vector3 position)
    {
        if (robot == null || gameObject == null)
        {
            return;
        }

        // Allow user to select a new goal and clear the previous goal <-- FIX
        if (selectedObject != null)
        {
            highlightObject.RemoveHighlight(selectedObject);
            autoGrasping.CancelCurrentTargetObject();
            armController.CancelAutonomyTarget();
        }

        selectedObject = gameObject;
        hasNewGoal = true;
        highlightObject.Highlight(selectedObject, cam, displayRect);
    }

    private void TaskSchedule()
    {
        switch (currentState)
        {
            case AutonomyState.SelectObject:
                if (hasNewGoal)
                {
                    hasNewGoal = false;
                    reachedGoal = false;
                    (hoverTransform, graspTransform) = autoGrasping.GetHoverAndGraspTransforms(selectedObject);
                    currentState = AutonomyState.FirstHoverOverObject;
                }
                break;
            
            case AutonomyState.FirstHoverOverObject:
                if(gotTrajectory == false)
                {
                    //call it again
                    armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
                }
                 
                if (reachedGoal)
                {
                    Debug.Log("Reached hover goal");
                    reachedGoal = false;
                    armController.SetGripperPosition(0.0f);
                    currentState = AutonomyState.GraspObject;
                    Debug.Log("Switching state to grasp");
                }
                break;

            case AutonomyState.GraspObject:
                Debug.Log("Ready to grasp object");
                armController.SetAutonomyTarget(graspTransform.position, graspTransform.rotation);
                // armController.MoveToAutonomyTarget();
                if (reachedGoal)
                {
                    reachedGoal = false;
                    armController.SetGripperPosition(1.0f);
                    currentState = AutonomyState.SecondHoverOverObject;
                }
                break; 

            case AutonomyState.SecondHoverOverObject:
                armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
                armController.MoveToAutonomyTarget();
                if (reachedGoal)
                {
                    reachedGoal = false;
                    currentState = AutonomyState.DeliverToGoal;
                }
                break;
            
            case AutonomyState.DeliverToGoal:
                Debug.Log("Entered Goal Delivery");

                // Deliver to goal position <-- FIX
                goalObject = GameObject.Find("Wooden Box(Clone)");
                (hoverTransform, graspTransform) = autoGrasping.GetHoverAndGraspTransforms(goalObject);
                armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
                armController.MoveToAutonomyTarget();

                if (reachedGoal)
                {
                    armController.SetGripperPosition(0.0f);
                    Debug.Log("Exiting Goal Delivery");
                    currentState = AutonomyState.SelectObject;
                }
                break;
        }
    }

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
                armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
                armController.OnAutonomyComplete += OnArmReachedGoal;
            }
        }

        if (cam == null)
        {
            Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();
            if (cameras.Length > 0)
            {
                cam = cameras[0];

                objectSelector.SetCameraAndDisplay(cam, displayRect);
            }
        }

        // Allow user to stop arm motion and remove highlight only when in valid state <-- FIX
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentState == AutonomyState.SelectObject || currentState == AutonomyState.FirstHoverOverObject)
            {
                autoGrasping.CancelCurrentTargetObject();
                armController.CancelAutonomyTarget();
                highlightObject.RemoveHighlight(selectedObject);
            }
        }

        // State machine for tasks
        TaskSchedule();
    }
}