using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///    Base autonomy
///    Example Script to Handle the autonomy of the robot base
///    Goals:
///    - Be able to pass in the target position and orientation
///    - Be able to visualize the trajectory
///    - Be able to cancel the trajectory
///    - Identiy when the robot has reached the goal
/// </summary>
public class BaseAutonomy : MonoBehaviour
{

    // Test setup
    [SerializeField] private GraphicalInterface graphicalInterface;
    [SerializeField] private RectTransform displayRect;
    private Camera cam;

    // AR Featrues 
    [SerializeField] private FloorSelector floorSelector;
    [SerializeField] private DrawWaypoints drawWaypoints;

    // Autonomy
    [SerializeField] private ArticulationBaseController baseController;

    private GameObject robot;
    private Vector3 goalPosition;

    void OnEnable()
    {
        // Subscribe to the event
        floorSelector.OnFloorSelected += OnFloorSelected;
        // objectSelector.OnObjectSelected += OnObjectSelected;

        if (baseController != null)
        {
            baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete += OnBaseReachedGoal;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up
        floorSelector.OnFloorSelected -= OnFloorSelected;
        // objectSelector.OnObjectSelected -= OnObjectSelected;

        if (baseController != null)
        {
            baseController.OnAutonomyTrajectory -= OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete -= OnBaseReachedGoal;
        }
    }

    private void OnFloorSelected(Vector3 position, Quaternion rotation)
    {
        if (robot == null)
        {
            return;
        }

        Debug.Log("Floor selected: " + position + " " + rotation);

        baseController.SetAutonomyTarget(position, rotation);
        goalPosition = position;
    }


    private void OnBaseTrajectoryGenerated()
    {
        Debug.Log("Base trajectory generated");

        var (globalWaypoints, LocalWaypoints) = 
            baseController.GetTrajectories();
        
        // Clear old waypoints
        drawWaypoints.RemoveLine("Global Path");
        // Add new waypoints
        drawWaypoints.DrawLine("Global Path", globalWaypoints);
    }

    private void OnBaseReachedGoal()
    {
        // Event will be called when the robot reaches the goal
        Debug.Log("Base reached goal");
    }

    // Use the robot world position and the goal position to calculate when we reached the goal
    private void CheckHasReachedGoal()
    {
        if(Vector3.Distance(robot.transform.position, goalPosition) < 0.2f)
        {
            Debug.Log("Base reached goal: Using CheckHasReachedGoal()");
            baseController.CancelAutonomyTarget();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get the main camera if it is not already set
        if (cam == null)
        {
            // Get all the active cameras referanced in the graphical interface
            Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();
            if (cameras.Length > 0)
            {
                cam = cameras[0];

                //////////////////////////////////////////
                floorSelector.SetCameraAndDisplay(cam, displayRect);
                // objectSelector.SetCameraAndDisplay(cam, displayRect);
                //////////////////////////////////////////
            }
        }

        // Find the Gopher GameObject
        robot = GameObject.Find("Gopher(Clone)");
        if (robot != null)
        {
            // Set the articulation base controller
            baseController = robot.GetComponentInChildren<ArticulationBaseController>();

            // Constantly subscribe to the event to make our trajectory visible
            //      check if we arrive at the goal
            baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete += OnBaseReachedGoal;
            CheckHasReachedGoal();

        }

        // Keyboard press enter to start autonomy
        if (Input.GetKeyDown(KeyCode.Return))
        {
            baseController.MoveToAutonomyTarget();
        }

        // Keyboard press space to emergency stop
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Emergency Stop");
            baseController.CancelAutonomyTarget();
        }

    }
}
