using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Autonomy for 2D navigation.
///
///     The global planner used is the default
///     Unity A* algorithm from NavMesh.
///     NavMesh is necessary for this planner.
///
///     The local planning strategy now is simply
///     "rotate then move forward".
/// </summary>
public class UnityAutoNavigation : AutoNavigation
{
    // Robot
    public GameObject robot;
    public ArticulationBaseController baseController;
    
    // TODO arm control
    /*
    public ArmControlManager leftArmControlManager;
    public ArmControlManager rightArmControlManager;
    */
    
    // nav mesh agent
    // this is not actually used, only served as the parameter container
    // for speed, angularSpeed, stoppingDistance, areMask, etc.
    public NavMeshAgent agent;
    // nav mesh obstacles
    // during auto navigation, only the base one is enabled
    private NavMeshObstacle[] navMeshObstacles;
    private Coroutine planningCoroutine;

    // Motion planning
    public float replanTime = 5f;
    private float elapsed;
    private Vector3 goal = new(0f, -100f, 0f);
    private NavMeshPath path;
    private int waypointIndex = 0;
    private bool rotationNeeded = true;

    void Start()
    {
        // never enabled
        agent.enabled = false;
    }

    void Update() {}

    void FixedUpdate()
    {
        // Check goals
        if (GlobalWaypoints.Length == 0)
            return;

        // Check replan
        elapsed += Time.fixedDeltaTime;
        if (elapsed > replanTime)
        {
            // replan
            elapsed = 0f;
            SetGoal(goal);
        }

        // Autonomy disabled
        if (!IsNavigating)
            return;

        // Check goal
        // select tolerance
        float tolerance = 0.1f;
        if (waypointIndex == GlobalWaypoints.Length - 1)
            tolerance = agent.stoppingDistance;
        // move to current waypoint
        float currentDis = (transform.position - GlobalWaypoints[waypointIndex]).magnitude;

        // Check distance to GlobalWaypoints and update motion
        if (currentDis > tolerance)
        {
            NavigateToWaypoint(GlobalWaypoints[waypointIndex]);
        }
        // current waypoint reached
        else
        {
            waypointIndex ++;
            // prevDis = 0; // temp
            rotationNeeded = true;
            // Fianl goal is reached
            if (waypointIndex == GlobalWaypoints.Length)
            {
                baseController.SetVelocity(Vector3.zero, Vector3.zero);
                StopNavigation();
            }
        }
    }

    private void NavigateToWaypoint(Vector3 waypoint)
    {
        // P controller, Kp = 2
        float Kp = 2;
        // Errors
        float distance = (waypoint - transform.position).magnitude;
        Quaternion targetRotation = Quaternion.LookRotation(waypoint - transform.position);
        float angleDifference = Mathf.DeltaAngle(targetRotation.eulerAngles[1], 
                                                 transform.rotation.eulerAngles[1]);

        // Adjust rotation angle first
        if (Mathf.Abs(angleDifference) > 1 && rotationNeeded) // 1° tolorance
        {
            float angularSpeed = Kp * angleDifference;
            angularSpeed = Mathf.Clamp(angularSpeed, -agent.angularSpeed, agent.angularSpeed);
            // set angular velocity
            Vector3 angularVelocity = new Vector3(0f, angularSpeed * Mathf.Deg2Rad, 0f);
            baseController.SetVelocity(Vector3.zero, angularVelocity);
        }
        // Then handle by local planner
        else
        {
            rotationNeeded = false;
            
            float linearSpeed = Kp * distance;
            linearSpeed = Mathf.Clamp(linearSpeed, -agent.speed, agent.speed);
            // set linear speed
            Vector3 linearVelocity = new Vector3(0f, 0f, linearSpeed);
            baseController.SetVelocity(linearVelocity, Vector3.zero);
        }
    }

    // TODO orientation
    public override void SetGoal(Vector3 position)
    {
        SetGoal(position, Vector3.zero);
    }

    // TODO orientation is not used yet
    public override void SetGoal(Vector3 position, Vector3 orientation)
    {
        // Get closest point in the navmesh
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1.0f, agent.areaMask))
        {
            Vector3 goal = hit.position;

            // path planning
            if (planningCoroutine != null)
            {
                StopCoroutine(planningCoroutine);
            }
            planningCoroutine = StartCoroutine(PathPlanningCoroutine(goal));
        }
        else
        {
            Debug.Log("The given goal is invalid.");
        }

        // Don't move the robot before confirmation
        IsNavigating = false;
    }

    private IEnumerator PathPlanningCoroutine(
        Vector3 goal, float obstacleDisableTime = 0.5f)
    {
        SetObstacleActive(false);
        yield return new WaitForSeconds(0.1f);
        bool pathFound = FindGlobalPath(goal);

        // path found or not
        if (pathFound)
            this.goal = goal;
        else
            Debug.Log("No path found to given goal.");
        yield return new WaitForSeconds(obstacleDisableTime);
        SetObstacleActive(true);
    }

    private void SetObstacleActive(bool IsNavigating)
    {
        // In case arm is carrying objects
        navMeshObstacles = robot.GetComponentsInChildren<NavMeshObstacle>();
        foreach(NavMeshObstacle navMeshObstacle in navMeshObstacles)
            navMeshObstacle.carving = IsNavigating;
    }

    private bool FindGlobalPath(Vector3 goal)
    {
        if (goal[1] == -100f)
            return false;

        // Global Path finding - A*
        path = new NavMeshPath();
        NavMesh.CalculatePath(
            transform.position, goal, agent.areaMask, path
        );
        // Set trajectories
        SetTrajectory(path);
        waypointIndex = 0;
        rotationNeeded = true;

        return path.corners.Length != 0;
    }

    private void SetTrajectory(NavMeshPath path)
    {
        // Convert path into GlobalWaypoints
        GlobalWaypoints = new Vector3[path.corners.Length];
        // Invalid Trajectory -> stop sign
        if (path.corners.Length == 0)
        {
            return;
        }
        // Valid -> Set up GlobalWaypoints and goal
        for (int i = 0; i < path.corners.Length; ++i)
        {
            GlobalWaypoints[i] = path.corners[i];
        }
    }

    // Start navigation
    public override void StartNavigation()
    {
        ResumeNavigation();
    }

    // Resume navigation
    // TODO: arm pose
    public override void ResumeNavigation()
    {
        // Must have valid goal and plan first
        /*
        if (goal[1] == -100f)
        {
            Debug.Log("No valid goal is set.");
            return;
        }
        */
        /*
        // Change arm pose
        bool changeArmPose = true;
        if (changeArmPose)
        {
            bool success = ChangeArmPose(5);
            Debug.Log("Changing arm pose failed.");
            if (!success)
                return;
        }
        */
        IsNavigating = true;
    }

    // TODO: arm pose
    /*
    private bool ChangeArmPose(int presetIndex)
    {
        bool leftSuccess = true;
        bool rightSuccess = true;
        if (leftArmControlManager != null && rightArmControlManager != null)
        {
            leftSuccess = leftArmControlManager.MoveToPreset(presetIndex);
            rightSuccess = rightArmControlManager.MoveToPreset(presetIndex);
        }
        return leftSuccess && rightSuccess;
    }
    */

    public override void PauseNavigation()
    {
        // Stop moving the robot
        baseController.SetVelocity(Vector3.zero, Vector3.zero);
        IsNavigating = false;
    }

    // Terminate navigation
    public override void StopNavigation()
    {
        // Init parameters
        goal = new Vector3(0f, -100f, 0f);
        path = new NavMeshPath();
        GlobalWaypoints = new Vector3[0];
        SetTrajectory(path);

        // Stop moving the robot
        baseController.SetVelocity(Vector3.zero, Vector3.zero);
        IsNavigating = false;
    }
}