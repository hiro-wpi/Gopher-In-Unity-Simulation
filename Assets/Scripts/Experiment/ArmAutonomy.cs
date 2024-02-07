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
    private GameObject selectedObject;
    private GameObject previousSelectedObject;
    private Transform hoverTransform;
    private Transform graspTransform;
    private Transform goalHoverTransform;
    private Transform goalGraspTransform;
    [SerializeField] private bool hasNewGoal = false;
    private GameObject goalObject;

    private enum AutonomyState
    {
        FirstHoverOverObject,
        GraspObject,
        SecondHoverOverObject,
        DeliverToGoal,
        OpenGripperAtGoal
    }
    private AutonomyState currentState;

    void Start()
    {
        armWaypointParent = new GameObject("Arm Waypoints");
        armWaypointParent.transform.SetParent(transform);
        armWaypointParent.transform.localPosition = Vector3.zero;
        armWaypointParent.transform.localRotation = Quaternion.identity;

        currentState = AutonomyState.FirstHoverOverObject;
    }

    void OnEnable()
    {
        // Subscribe to the event
        objectSelector.OnObjectSelected += OnObjectSelected;

        if (armController != null)
        {
            armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
            // armController.OnAutonomyComplete += OnArmAutonomyComplete;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up
        objectSelector.OnObjectSelected -= OnObjectSelected;

        if (armController != null)
        {
            armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
            // armController.OnAutonomyComplete += OnArmAutonomyComplete;
        }
    }

    // private void OnArmAutonomyComplete()
    // {
    //     Debug.Log("Arm Autonomy Complete");
    //     foreach (Transform child in armWaypointParent.transform)
    //     {
    //         arGenerator.Destroy(child.gameObject);
    //         Destroy(child.gameObject);
    //     }
    // }
    
    private void OnArmTrajectoryGenerated()
    {
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
    }

    private void OnObjectSelected(GameObject gameObject, Vector3 position)
    {
        if (robot == null || gameObject == null)
        {
            return;
        }
        
        GameObject previousObject = selectedObject;

        // Allow user to select a new goal and clear the previous goal <-- FIX
        if (selectedObject != null)
        {
            autoGrasping.CancelCurrentTargetObject();
            armController.CancelAutonomyTarget();
            highlightObject.RemoveHighlight(previousObject);
            armController.SetGripperPosition(0.0f);
            hasNewGoal = true;
            currentState = AutonomyState.FirstHoverOverObject;
        }

        selectedObject = gameObject;
        highlightObject.Highlight(selectedObject, cam, displayRect);
        (hoverTransform, graspTransform) = autoGrasping.GetHoverAndGraspTransforms(selectedObject);
        hasNewGoal = true;

        previousSelectedObject = selectedObject;
    }

    private void TaskSchedule()
    {
        switch (currentState)
        {   
            case AutonomyState.FirstHoverOverObject:
                armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
                if (hasNewGoal && Input.GetKeyDown(KeyCode.Space))
                {
                    hasNewGoal = false;
                    armController.MoveToAutonomyTarget();
                    currentState = AutonomyState.GraspObject;
                }
                break;

            case AutonomyState.GraspObject:
                armController.SetAutonomyTarget(graspTransform.position, graspTransform.rotation);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    armController.MoveToAutonomyTarget();
                    currentState = AutonomyState.SecondHoverOverObject;
                }
                break; 

            case AutonomyState.SecondHoverOverObject:
                armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
                
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    armController.SetGripperPosition(1.0f);
                    armController.MoveToAutonomyTarget();
                    currentState = AutonomyState.DeliverToGoal;
                }
                break;
            
            case AutonomyState.DeliverToGoal:
                armController.SetAutonomyTarget(goalHoverTransform.position, goalHoverTransform.rotation);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    armController.MoveToAutonomyTarget();
                    currentState = AutonomyState.OpenGripperAtGoal;
                }
                break;
            
            case AutonomyState.OpenGripperAtGoal:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    armController.SetGripperPosition(0.0f);
                    highlightObject.RemoveHighlight(selectedObject);
                    currentState = AutonomyState.FirstHoverOverObject;
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

        goalObject = GameObject.Find("Experiment Objects/Goal Medicine");
        (goalHoverTransform, goalGraspTransform) = autoGrasping.GetHoverAndGraspTransforms(goalObject);

        // Allow user to stop arm motion and remove highlight only when in valid state <-- FIX
        if (Input.GetKeyDown(KeyCode.Return))
        {
            autoGrasping.CancelCurrentTargetObject();
            armController.CancelAutonomyTarget();
            highlightObject.RemoveHighlight(selectedObject);
            armController.SetGripperPosition(0.0f);
            hasNewGoal = false;
            currentState = AutonomyState.FirstHoverOverObject;
        }

        // State machine for tasks
        TaskSchedule();
    }
}