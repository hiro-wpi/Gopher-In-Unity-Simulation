using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARManipulationAutomation : MonoBehaviour
{
    // Start is called before the first frame update

    // Task setup
    private GameObject robot;
    [SerializeField] private string robotName = "Gopher";

    // AR Featrues
    [SerializeField] private GenerateARGameObject arGenerator;
    private GameObject armWaypointParent;
    [SerializeField] private GameObject arGripper;

    // Automation

    [SerializeField] private ArticulationArmController armController;
    [SerializeField] private AutoGrasping autoGrasping;

    // Waypoints
    private List<int> waypointGripperActions = new List<int>(); // 0 for open, 1 for close
    // private GameObject leftRobotEE;
    public List<Vector3> waypointArmPositions = new List<Vector3>();
    public List<Quaternion> waypointArmRotations = new List<Quaternion>();

    // goal flags

    private bool waypointArmReachedGoal = false; // This is for just reaching successive points on the list
    [SerializeField, ReadOnly] public bool reachedArmGoal = false;  // reached the whole goal
    [SerializeField, ReadOnly] public bool autoReady = false;  // reached the whole goal

    

    void Start()
    {

        armWaypointParent = new GameObject("Arm Waypoints");
        armWaypointParent.transform.SetParent(transform);
        armWaypointParent.transform.localPosition = Vector3.zero;
        armWaypointParent.transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        // Get the main camera if it is not already set
        // if (cam == null)
        // {
        //     // Get all the active cameras referanced in the graphical interface
        //     Camera[] cameras =  graphicalInterface.GetCurrentActiveCameras();
        //     if (cameras.Length > 0)
        //     {
        //         cam = cameras[0];

        //         //////////////////////////////////////////
        //         // floorSelector.SetCameraAndDisplay(cam, displayRect);
        //         // objectSelector.SetCameraAndDisplay(cam, displayRect);
        //         //////////////////////////////////////////
        //     }
        // }

        if(robot == null)
        {
            robot = GameObject.Find(robotName + "(Clone)");
            
            // Set the articulation base controller
            if(robot == null)
            {
                // Debug.Log("No robot found");
                return;
            }
            // Get Child of the robot
            // leftRobotEE = robot.transform.Find("Plugins/Hardware/Left Arm").gameObject;

            // baseController = robot.GetComponentInChildren<ArticulationBaseController>();
            armController = robot.GetComponentInChildren<ArticulationArmController>();
            autoGrasping = robot.GetComponentInChildren<AutoGrasping>();
            // Constantly subscribe to the event to make our trajectory visible
            //      check if we arrive at the goal
            // baseController.OnAutonomyTrajectory += OnBaseTrajectoryGenerated;
            // baseController.OnAutonomyComplete += OnBaseReachedGoal;

            armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
            armController.OnAutonomyComplete += OnArmAutonomyComplete;

            autoReady = true;

            // ScheuldeNextTask();
        }

    }

    void OnEnable()
    {
        // Subscribe to the event

        if (armController != null)
        {
            armController.OnAutonomyTrajectory += OnArmTrajectoryGenerated;
            armController.OnAutonomyComplete += OnArmAutonomyComplete;
        }
    }

    void OnDisable()
    {
        // Unsubscribe from the static event to clean up

        if (armController != null)
        {
            armController.OnAutonomyTrajectory -= OnArmTrajectoryGenerated;
            armController.OnAutonomyComplete -= OnArmAutonomyComplete;
        }
    }

    // private void OnArmAutonomyComplete()
    // {
    //     // Debug.Log("Arm Autonomy Complete");
    //     waypointArmReachedGoal = true;
    // }

    // private void OnArmTrajectoryGenerated()
    // {


    //     // Debug.Log("Arm trajectory generated");
    //     armController.MoveToAutonomyTarget();
    // }

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

    IEnumerator FollowArmWaypoints()
    {
        for(int i = 0; i < waypointArmPositions.Count; i++)
        {
            // Reset the reached goal flag
            reachedArmGoal = false;
            waypointArmReachedGoal = false;

            // Move to the position
            armController.SetAutonomyTarget(waypointArmPositions[i], waypointArmRotations[i]);

            // Wait for when the arm reaches the position
            yield return new WaitUntil(() => waypointArmReachedGoal);

            // Set the gripper (open or closed)
            if(waypointGripperActions[i] != -1)
            {
                armController.SetGripperPosition(waypointGripperActions[i]);
            }

            // Wait for gripper to be done
            yield return new WaitForSeconds(0.5f);
            
        }
        reachedArmGoal = true;

        // Debug.Log("Finished Trajectory");
    }

    public void HomeJoints()
    {
        armController.HomeJoints();
    }

    public (Transform, Transform) GetHoverAndGraspTransforms(GameObject obj)
    {
        // Get the hover and grasp transforms
        return autoGrasping.GetHoverAndGraspTransforms(obj);
    }

    private void OnArmAutonomyComplete()
    {
        foreach (Transform child in armWaypointParent.transform)
        {
            arGenerator.Destroy(child.gameObject);
            Destroy(child.gameObject);
        }

        waypointArmReachedGoal = true;
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

        armController.MoveToAutonomyTarget();
    }

    
}
