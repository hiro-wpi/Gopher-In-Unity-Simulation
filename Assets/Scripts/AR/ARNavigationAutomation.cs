using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARNavigationAutomation : MonoBehaviour
{
     // Task setup
    // [SerializeField] private GraphicalInterface graphicalInterface;
    // [SerializeField] private RectTransform displayRect;
    // private Camera cam;
    private GameObject robot;

    // AR Featrues 
    // [SerializeField] private FloorSelector floorSelector;
    // [SerializeField] private DrawWaypoints drawLocalWaypoints;
    [SerializeField] private DrawWaypoints drawGlobalWaypoints;

    // private bool hideLocalPath = false;

    // Autonomy
    [SerializeField] private ArticulationBaseController baseController;

    // Tracked Trajectory of the Robot base
    private List<Vector3> waypointPositions = new List<Vector3>();
    private List<Vector3> waypointRotations = new List<Vector3>();

    private bool waypointReachedGoal = false; // This is for just reaching successive points on the list

    [SerializeField, ReadOnly] public bool reachedGoal = false;  // reached the whole goal
    [SerializeField, ReadOnly] public bool autoReady = false;  // reached the whole goal

    private bool passingInGlobalGoal = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // // Get the main camera if it is not already set
        // if (cam == null)
        // {
        //     // Get all the active cameras referanced in the graphical interface
        //     Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();
        //     if (cameras.Length > 0)
        //     {
        //         cam = cameras[0];
        //     }
        // }

        if(robot == null)
        {
            robot = GameObject.Find("Gopher(Clone)");
            
            // Set the articulation base controller
            if(robot == null)
            {
                // Debug.Log("No robot found");
                return;
            }
            // Get Child of the robot
            // leftRobotEE = robot.transform.Find("Plugins/Hardware/Left Arm").gameObject;

            baseController = robot.GetComponentInChildren<ArticulationBaseController>();
            
            // Constantly subscribe to the event to make our trajectory visible
            //      check if we arrive at the goal
            baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete += OnBaseReachedGoal;

            autoReady = true;
        }

    }

    void OnEnable()
    {
        // Subscribe to the event

        if (baseController != null)
        {
            baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete += OnBaseReachedGoal;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up

        if (baseController != null)
        {
            baseController.OnAutonomyTrajectory -= OnBaseTrajectoryGenerated;
            baseController.OnAutonomyComplete -= OnBaseReachedGoal;
        }
    }

    private void OnBaseReachedGoal()
    {
        // Event will be called when the robot reaches the goal
        waypointReachedGoal = true;
    }

        // Automatically Sets the waypoint and goes to it
    public void SetWaypoints(Vector3 pos, Vector3 rot)
    {
        waypointPositions = new List<Vector3>{pos};
        waypointRotations = new List<Vector3>{rot};

        StartCoroutine(FollowWaypoints());
    }

    // Automatically Sets the waypoint and goes to it
    public void SetWaypoints(List<Vector3> pos, List<Vector3> rot)
    {
        if(pos.Count != rot.Count)
        {
            Debug.LogWarning("The list used are not the same size, they are not being added");
            return;
        }

        waypointPositions = pos;
        waypointRotations = rot;

        StartCoroutine(FollowWaypoints());
    }

    IEnumerator FollowWaypoints()
    {
        // Debug.Log("Start Motion");

        // get the last position and rotation from waypointPositions and waypointRotations
        reachedGoal = false;

        int lastIndex = waypointPositions.Count - 1;

        // Display the global path
        passingInGlobalGoal = true;
        baseController.SetAutonomyTarget(waypointPositions[lastIndex], Quaternion.Euler(waypointRotations[lastIndex]));
        yield return new WaitUntil(() => passingInGlobalGoal == false);
        
        for(int i = 0; i < waypointPositions.Count; i++)
        {
            // Debug.Log("Going to Waypoint");
            // Reset the reached goal flag
            
            waypointReachedGoal = false;

            // Convert from euler angles to quaternion
            Quaternion worldRotation = Quaternion.Euler(waypointRotations[i]);
            baseController.SetAutonomyTarget(waypointPositions[i], worldRotation);

            yield return new WaitUntil(() => waypointReachedGoal);
        }
        reachedGoal = true;
    }


    private void OnBaseTrajectoryGenerated()
    {
        // Debug.Log("Base trajectory generated");
        var (globalWaypoints, LocalWaypoints) = 
            baseController.GetTrajectories();

        // Generate the global path
        if(passingInGlobalGoal == true)
        {   
            // Clear old waypoints
            drawGlobalWaypoints.RemoveLine("Global Path");
            // Add new waypoints
            drawGlobalWaypoints.DrawLine("Global Path", globalWaypoints);
            
        }
        else
        {

            // Automatically Send the robot to the goal
            baseController.MoveToAutonomyTarget();
        }
        passingInGlobalGoal = false;
    }

    public void CancelAutonomy()
    {
        baseController.CancelAutonomyTarget();
    }

}
