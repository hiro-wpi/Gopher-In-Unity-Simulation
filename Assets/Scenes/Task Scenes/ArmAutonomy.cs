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
    private bool planFlag = true;
    private bool completed = false;

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
            armController.OnAutonomyComplete += OnAutonomyCompleted;
        }
    }

    void OnDisable()
    {
        objectSelector.OnObjectSelected -= OnObjectSelected;

        if (armController != null)
        {
            armController.OnAutonomyTrajectory -= OnArmTrajectoryGenerated;
            armController.OnAutonomyComplete -= OnAutonomyCompleted;
        }
    }
    
    private void OnArmTrajectoryGenerated()
    {
        var (time, angles, velocities, accelerations) = 
            armController.GetAutonomyTrajectory();

        foreach (Transform child in armWaypointParent.transform)
        {
            arGenerator.Destroy(child.gameObject);
            Destroy(child.gameObject);
        }

        for (int i = 0; i < time.Length; i++)
        {
            GameObject waypoint;
            if (i == 0 || i == time.Length - 1)
            {
                waypoint = Instantiate(arGripper);
            }
            else
            {
                waypoint = new GameObject("waypoint" + i);
            }
            

            waypoint.transform.SetParent(armWaypointParent.transform);

            (waypoint.transform.position, waypoint.transform.rotation) =
                armController.GetEETargetPose(angles[i]);
            
            if (i == 0 || i == time.Length - 1)
            {
                arGenerator.Instantiate(
                    waypoint,
                    arGripper
                );
            }
            else 
            {
                arGenerator.Instantiate(
                    waypoint,
                    GenerateARGameObject.ARObjectType.Sphere,
                    color: Color.red,
                    transparency: 0.35f,
                    scale: new Vector3(0.05f, 0.05f, 0.05f)
                );
            }
        }
    }

    private void OnAutonomyCompleted()
    {
        completed = true;

        foreach (Transform child in armWaypointParent.transform)
        {
            arGenerator.Destroy(child.gameObject);
            Destroy(child.gameObject);
        }        
    }

    private void OnObjectSelected(GameObject gameObject, Vector3 position)
    {
        if (robot == null || gameObject == null)
        {
            return;
        }
        
        GameObject previousObject = selectedObject;

        if (selectedObject != null)
        {
            autoGrasping.CancelCurrentTargetObject();
            armController.CancelAutonomyTarget();
            highlightObject.RemoveHighlight(previousObject);
            armController.SetGripperPosition(0.0f);
            hasNewGoal = true;
            planFlag = true;
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
                if (selectedObject == null)
                {
                    return;
                }

                if (planFlag)
                {
                    armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
                    planFlag = false;
                    completed = false;                    
                }

                if (hasNewGoal && Input.GetKeyDown(KeyCode.Space))
                {
                    hasNewGoal = false;
                    armController.MoveToAutonomyTarget();
                }

                if (completed)
                {
                    currentState = AutonomyState.GraspObject;
                    planFlag = true;
                }
                break;

            case AutonomyState.GraspObject:
                if (planFlag)
                {
                    armController.SetAutonomyTarget(graspTransform.position, graspTransform.rotation);
                    planFlag = false;
                    completed = false;                    
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    hasNewGoal = false;
                    armController.MoveToAutonomyTarget();
                }

                if (completed)
                {
                    currentState = AutonomyState.SecondHoverOverObject;
                    planFlag = true;
                }
                break; 

            case AutonomyState.SecondHoverOverObject:
                if (planFlag)
                {
                    armController.SetAutonomyTarget(hoverTransform.position, hoverTransform.rotation);
                    planFlag = false;
                    completed = false;                    
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    armController.SetGripperPosition(1.0f);
                    hasNewGoal = false;
                    armController.MoveToAutonomyTarget();
                }

                if (completed)
                {
                    currentState = AutonomyState.DeliverToGoal;
                    planFlag = true;
                }
                break;
            
            case AutonomyState.DeliverToGoal:
                if (planFlag)
                {
                    armController.SetAutonomyTarget(goalHoverTransform.position, goalHoverTransform.rotation);
                    planFlag = false;
                    completed = false;                    
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    hasNewGoal = false;
                    armController.MoveToAutonomyTarget();
                }

                if (completed)
                {
                    currentState = AutonomyState.OpenGripperAtGoal;
                    planFlag = true;
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
            robot = GameObject.Find("Gopher Manipulation(Clone)");
            if (robot != null)
            {
                Transform rightArmHardware = robot.transform.Find("Plugins/Hardware/Right Arm");
                Transform rightArmAutonomy = robot.transform.Find("Plugins/Autonomy/Unity/Right Arm");

                armController = rightArmHardware.GetComponentInChildren<ArticulationArmController>();
                autoGrasping = rightArmAutonomy.GetComponentInChildren<AutoGrasping>();

                armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
                armController.OnAutonomyComplete += OnAutonomyCompleted;
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
        
        if (goalObject == null)
        {
            goalObject = GameObject.Find("Experiment Objects/Goal Medicine");
        }
        if (goalObject != null && autoGrasping != null)
        {
            (goalHoverTransform, goalGraspTransform) = autoGrasping.GetHoverAndGraspTransforms(goalObject);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            autoGrasping.CancelCurrentTargetObject();
            armController.CancelAutonomyTarget();
            highlightObject.RemoveHighlight(selectedObject);
            armController.SetGripperPosition(0.0f);
            hasNewGoal = false;
            planFlag = true;
            currentState = AutonomyState.FirstHoverOverObject;
        }

        TaskSchedule();
    }
}