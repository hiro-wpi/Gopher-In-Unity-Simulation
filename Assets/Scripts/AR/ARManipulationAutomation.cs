using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARManipulationAutomation : MonoBehaviour
{
    // Start is called before the first frame update

    // Task setup
    private GameObject robot;

    // Automation
    private ArticulationArmController armController;
    private AutoGrasping autoGrasping;

    // AR Featrues
    [SerializeField] private GenerateARGameObject arGenerator;

    // Waypoints
    private List<int> waypointGripperActions = new List<int>(); // 0 for open, 1 for close
    private List<Vector3> waypointArmPositions = new List<Vector3>();
    private List<Quaternion> waypointArmRotations = new List<Quaternion>();

    // Goal flags
    private bool waypointReachedGoal = false; // This is for just reaching successive points in the trajectory
    [ReadOnly] public bool reachedGoal = false;  // For reaching the whole trajectory

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(robot == null)
        {
            robot = GameObject.Find("Gopher(Clone)");
            
            // Set the articulation base controller
            if(robot == null)
            {
                return;
            }
            
            // Get robot components
            armController = robot.GetComponentInChildren<ArticulationArmController>();
            autoGrasping = robot.GetComponentInChildren<AutoGrasping>();
            
            armController.OnAutonomyTrajectory += OnArmTrajectoryWaypointGenerated;
            armController.OnAutonomyComplete += OnArmAutonomyWaypointReached;

        }

    }

    void OnEnable()
    {
        // Subscribe to the events for the arm controller

        if (armController != null)
        {
            armController.OnAutonomyTrajectory += OnArmTrajectoryWaypointGenerated;
            armController.OnAutonomyComplete += OnArmAutonomyWaypointReached;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up

        if (armController != null)
        {
            armController.OnAutonomyTrajectory -= OnArmTrajectoryWaypointGenerated;
            armController.OnAutonomyComplete -= OnArmAutonomyWaypointReached;
        }
    }

    // Event Handlers //////////////////////////////////////////////////////////////////////
    // OnArmAutonomyWaypointReached - This is called when the arm reaches a waypoint
    private void OnArmAutonomyWaypointReached()
    {
        // Debug.Log("Arm Autonomy Complete");

        waypointReachedGoal = true;
    }

    // OnArmAutonomyComplete - This is called when the arm is able to create a trajectory
    private void OnArmTrajectoryWaypointGenerated()
    {
        // Debug.Log("Arm trajectory generated");

        // Automatically move to the target
        armController.MoveToAutonomyTarget();
    }

    // SetArmWaypoints - Set the waypoint(s) for the arm, and how to handle gripper actions
    public void SetArmWaypoints(Vector3 position, Quaternion rotation, int gripperAction = 0)
    {
        SetArmWaypoints(new List<Vector3>{position}, new List<Quaternion>{rotation}, new List<int>{gripperAction});
    }


    public void SetArmWaypoints(List<Vector3> positions, List<Quaternion> rotations, List<int> gripperActions)
    {
        if(positions.Count != rotations.Count || positions.Count != gripperActions.Count)
        {
            Debug.LogWarning("The list used are not the same size, they are not being added");
            return;
        }

        waypointArmPositions = positions;
        waypointArmRotations = rotations;
        waypointGripperActions = gripperActions;

        StartCoroutine(FollowArmWaypoints());
    }

    // FollowArmWaypoints - Follow the waypoints for the arm and manages gripper actions
    // - Order of operations:
    //   1. Move to the position
    //   2. Wait for when the arm reaches the position
    //   3. Set the gripper (open or closed, if needed)
    //      - (-1) for no action
    //      - (0) for open
    //      - (1) for close
    //   4. Repeat for the next position until the end
    IEnumerator FollowArmWaypoints()
    {
        for(int i = 0; i < waypointArmPositions.Count; i++)
        {
            // Reset the reached goal flag
            reachedGoal = false;
            waypointReachedGoal = false;

            // Move to the position
            armController.SetAutonomyTarget(waypointArmPositions[i], waypointArmRotations[i]);

            // Wait for when the arm reaches the position
            yield return new WaitUntil(() => waypointReachedGoal);

            // Set the gripper (open or closed)
            if(waypointGripperActions[i] != -1)
            {
                armController.SetGripperPosition(waypointGripperActions[i]);
            }
            
        }
        reachedGoal = true;

        Debug.Log("Finished Trajectory");
    }

    // HomeJoints - Home the joints of the arm
    public void HomeJoints()
    {
        armController.HomeJoints();
    }

    // GetHoverAndGraspTransforms - Get the hover and grasp transforms for the object
    // - HoverTransform: Someplace close to the object, to make it easier to grasp
    // - GraspTransform: The place to grasp the object
    public (Transform, Transform) GetHoverAndGraspTransforms(GameObject obj)
    {
        // Get the hover and grasp transforms
        return autoGrasping.GetHoverAndGraspTransforms(obj);
    }
}
