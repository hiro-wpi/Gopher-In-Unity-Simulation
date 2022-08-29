using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     This script is used to navigate robot to a desired
///     goal automatically. The global planner used is the default
///     Unity A* algorithm from Nav Mesh. The local planning 
///     strategy now is simply "rotate then move forward".
/// </summary>
public class AutoNavigation : MonoBehaviour
{
    public bool active;

    // Robot
    public GameObject robot;
    public ArticulationWheelController wheelController;
    public ArmControlManager leftArmControlManager;
    public ArmControlManager rightArmControlManager;
    // nav mesh agent
    // this is not actually used, only served as the parameter container
    // for speed, angularSpeed, stoppingDistance, areMask, etc.
    public NavMeshAgent agent;
    // nav mesh obstacles
    // during auto navigation, only the base one is enabled
    public NavMeshObstacle[] baseNavMeshObstacles;
    public NavMeshObstacle[] armNavMeshObstacles;
    private Coroutine planningCoroutine;

    // Motion planning
    public float replanTime = 5f;
    private float elapsed;
    private Vector3 goal = new Vector3(0f, -100f, 0f);
    private NavMeshPath path;
    private Vector3[] waypoints = new Vector3[0];
    private int waypointIndex = 0;
    private bool rotationNeeded = true;
    /*
    // temp - could be removed after controller is implemented
    private float prevDis = 0f;
    private float errorCheckTime;
    private float errorCheckFreq = 1.0f;
    */
    
    // Visualization
    public GameObject goalPrefab;
    private GameObject goalObject;
    public LineRenderer lineRenderer;
    public bool drawPathEnabled = true;


    void Start()
    {
        // never enabled
        agent.enabled = false;
    }

    void Update()
    {
        // Path visualization
        if (!drawPathEnabled || waypoints.Length == 0)
        {
            lineRenderer.positionCount = 0;
            if (goalObject != null)
                Destroy(goalObject);
            return;
        }
        // Draw current point + waypoints
        lineRenderer.positionCount = (1 + waypoints.Length - waypointIndex);
        lineRenderer.SetPosition(0, transform.position);
        for (int i = 0; i < waypoints.Length - waypointIndex; ++i)
        {
            // higher for better visualization
            lineRenderer.SetPosition(1 + i, waypoints[i + waypointIndex] + 
                                            new Vector3(0f, 0.2f, 0f) ); 
        }
        // Draw goal
        if (goalObject != null && 
            goalObject.transform.position != waypoints[waypoints.Length-1])
        {
            Destroy(goalObject);
        }
        if (goalObject == null)
        {
            goalObject = Instantiate(goalPrefab,
                                     waypoints[waypoints.Length-1], 
                                     Quaternion.identity);
            Utils.SetGameObjectLayer(goalObject, "Robot", true);
        }
    }

    void FixedUpdate()
    {
        // Autonomy disabled
        if (!active)
            return;
        // Check goals
        if (waypoints.Length == 0)
            return;
        
        // Check replan
        elapsed += Time.fixedDeltaTime;
        if (elapsed > replanTime)
        {
            // replan
            elapsed = 0f;
            SetGoal(this.goal);
        }

        // Check goal
        // select tolerance
        float tolerance = 0.1f;
        if (waypointIndex == waypoints.Length - 1)
            tolerance = agent.stoppingDistance;
        // move to current waypoint
        float currentDis = (transform.position - waypoints[waypointIndex]).magnitude;
        
        /*
        // temp - Check if the robot is approaching the waypoint
        if (prevDis == 0)
            prevDis = currentDis;
        errorCheckTime += Time.fixedDeltaTime;
        if (errorCheckTime > errorCheckFreq)
        {
            errorCheckTime = 0;
            if ((currentDis - prevDis) > agent.speed*errorCheckFreq/2f)
            {
                SetGoal(this.goal);
                prevDis = 0;
            }
            prevDis = currentDis;
        }
        */

        // Check distance to waypoints and update motion
        if (currentDis > tolerance)
        {
            NavigateToWaypoint(waypoints[waypointIndex]);
        }
        // current waypoint reached
        else
        {
            waypointIndex ++;
            // prevDis = 0; // temp
            rotationNeeded = true;
            // Fianl goal is reached
            if (waypointIndex == waypoints.Length)
            {
                wheelController.SetRobotVelocity(0f, 0f);
                DisableAutonomy();
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
            // set angular speed
            float angularSpeed = Kp * angleDifference;
            angularSpeed = Mathf.Clamp(angularSpeed, -agent.angularSpeed, agent.angularSpeed);
            wheelController.SetRobotVelocity(0f, angularSpeed * Mathf.Deg2Rad);
        }
        // Then handle by local planner
        else
        {
            rotationNeeded = false;
            // set linear speed
            float lineawrSpeed = Kp * distance;
            lineawrSpeed = Mathf.Clamp(lineawrSpeed, -agent.speed, agent.speed);
            wheelController.SetRobotVelocity(lineawrSpeed, 0f);
        }
    }
    

    public void EnableAutonomy(bool changeArmPose = true)
    {
        // Must have valid goal and plan first
        if (goal[1] == -100f)
        {
            Debug.Log("No valid goal is set");
            return;
        }

        // Change nav mesh obsatcle usage
        SetObstacleActive(true, false);
        // Change arm pose
        if (changeArmPose)
            ChangeArmPose(5);
        active = true;
    }
    private void ChangeArmPose(int presetIndex)
    {
        if (leftArmControlManager != null && rightArmControlManager != null)
        {
            leftArmControlManager.MoveToPreset(presetIndex);
            rightArmControlManager.MoveToPreset(presetIndex);
        }
    }

    public void DisableAutonomy()
    {
        // Resume nav obsatcle
        SetObstacleActive(true, true);
        // Init parameters
        goal = new Vector3(0f, -100f, 0f);
        path = new NavMeshPath();
        waypoints = new Vector3[0];
        SetTrajectory(path);
        active = false;
    }

    private void SetObstacleActive(bool baseActive, bool armActive)
    {
        foreach(NavMeshObstacle navMeshObstacle in baseNavMeshObstacles)
            navMeshObstacle.carving = baseActive;
        foreach(NavMeshObstacle navMeshObstacle in armNavMeshObstacles)
            navMeshObstacle.carving = armActive;
    }

    
    public void SetGoal(Vector3 goal)
    {
        // Get closest point in the nav mesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(goal, out hit, 1f, agent.areaMask))
        {
            goal = hit.position;
            // prevent nav mesh obstacles blocking path
            if (planningCoroutine != null)
                StopCoroutine(planningCoroutine);
            planningCoroutine = StartCoroutine(PathPlanningCoroutine(0.5f, goal));
            // path planning
        }
        else
        {
            Debug.Log("The given goal is invalid.");
        }
    }
    private IEnumerator PathPlanningCoroutine(float time, Vector3 goal)
    {
        SetObstacleActive(false, false);
        yield return new WaitForSeconds(0.1f);
        bool pathFound = FindGlobalPath(goal);
        // path found or not
        if (pathFound)
        {
            this.goal = goal;
        }
        else
        {
            Debug.Log("No path found to given goal.");
        }
        yield return new WaitForSeconds(time);
        SetObstacleActive(true, true);
    }
    private bool FindGlobalPath(Vector3 goal)
    {
        if (goal[1] == -100f)
            return false;

        // Global Path finding - A*
        path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, 
                              goal, agent.areaMask, path);
        if (path.corners.Length == 0)
            return false;
        // Set trajectories
        SetTrajectory(path);
        waypointIndex = 0;
        rotationNeeded = true;
        return true;
    }
    private void SetTrajectory(NavMeshPath path)
    {
        // Convert path into waypoints
        waypoints = new Vector3[path.corners.Length];
        // Invalid Trajectory -> stop sign
        if (path.corners.Length == 0)
        {
            wheelController.SetRobotVelocity(0f, 0f);
            return;
        }
        // Valid -> Set up waypoints and goal
        for (int i = 0; i < path.corners.Length; ++i)
        {
            waypoints[i] = path.corners[i];
        }
    }
}
