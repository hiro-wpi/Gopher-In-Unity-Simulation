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
    // robot
    public GameObject robot;
    public ArticulationWheelController wheelController;
    public ArmControlManager leftArmControlManager;
    public ArmControlManager rightArmControlManager;
    // nav mesh agent - use the parameters there,
    // but not the agent itself
    public NavMeshAgent agent;
    public NavMeshObstacle[] navMeshObstacles;
    // motion planning
    public NavMeshSurface navMeshSurface;
    public Vector3 goal;
    public float replanTime = 5f;
    private float elapsed;
    private NavMeshPath path;
    private Vector3[] waypoints = new Vector3[0];
    private int waypointIndex = 0;
    private bool rotationNeeded = true;
    // visualization
    public LineRenderer lineRenderer;
    public bool drawPathEnabled;


    void Start()
    {
        if (agent == null)
            agent = gameObject.GetComponent<NavMeshAgent>();
        if (navMeshObstacles == null)
            navMeshObstacles = robot.GetComponentsInChildren<NavMeshObstacle>();
    }

    void Update()
    {
        // Path visualization
        if (!drawPathEnabled || waypoints.Length == 0)
        {
            lineRenderer.positionCount = 0;
            return;
        }
        // current point + waypoints
        lineRenderer.positionCount = (1 + waypoints.Length - waypointIndex);
        lineRenderer.SetPosition(0, agent.transform.position);
        for (int i = 0; i < waypoints.Length - waypointIndex; ++i)
        {
            // draw higher for better visualization
            lineRenderer.SetPosition(1 + i, waypoints[i + waypointIndex] + 
                                            new Vector3(0f, 0.2f, 0f) ); 
        }
    }

    void FixedUpdate()
    {
        // Check goals
        if (waypoints.Length == 0)
            return;
        
        // Check replan
        elapsed += Time.fixedDeltaTime;
        if (elapsed > replanTime)
        {
            // replan
            NavigateToGoal();
        }

        // Check distance to waypoints and update motion
        float tolerance = 0.05f;
        if (waypointIndex == waypoints.Length - 1)
            tolerance = agent.stoppingDistance;
        // move to current waypoint
        if ((agent.transform.position - waypoints[waypointIndex]).magnitude 
            > tolerance)
        {
            NavigateToWaypoint(waypoints[waypointIndex]);
        }
        // current waypoint reached
        else
        {
            waypointIndex ++;
            rotationNeeded = true;
            // Fianl goal is reached
            if (waypointIndex == waypoints.Length)
            {
                wheelController.SetRobotVelocity(0f, 0f);
                ClearGoal();
                ClearPath();
            }
        }
    }


    private void NavigateToWaypoint(Vector3 waypoint)
    {
        // P controller, Kp = 2
        float Kp = 2;
        // Errors
        float distance = (waypoint - agent.transform.position).magnitude;
        Quaternion targetRotation = Quaternion.LookRotation(waypoint - agent.transform.position);
        float angleDifference = Mathf.DeltaAngle(targetRotation.eulerAngles[1], 
                                                 agent.transform.rotation.eulerAngles[1]);

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
    

    public void EnableAutonomy()
    {
        // Change nav mesh usage
        foreach(NavMeshObstacle navMeshObstacle in navMeshObstacles)
            navMeshObstacle.enabled = false;
        agent.enabled = true;
        // Change arm pose
        ChangeArmPose(8);
    }
    public void DisableAutonomy()
    {
        // Change nav mesh usage
        agent.enabled = false;
        foreach(NavMeshObstacle navMeshObstacle in navMeshObstacles)
            navMeshObstacle.enabled = true;
        ClearGoal();
        ClearPath();
        // Change arm pose
        // ChangeArmPose(0);
    }
    private void ChangeArmPose(int presetIndex)
    {
        if (leftArmControlManager != null && rightArmControlManager != null)
        {
            leftArmControlManager.MoveToPreset(presetIndex);
            rightArmControlManager.MoveToPreset(presetIndex);
        }
    }


    public bool SetGoal(Vector3 goal)
    {
        if (!agent.enabled)
            return false;

        // Get closest point in the nav mesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(goal, out hit, 1f, agent.areaMask))
        {
            this.goal = hit.position;
            return true;
        }
        else
        {
            Debug.Log("The given goal is invalid");
            return false;
        }
    }

    public void NavigateToGoal()
    {
        if (goal[1] == float.NegativeInfinity)
            return;
        
        // Global Path finding - A*
        path = new NavMeshPath();
        NavMesh.CalculatePath(agent.transform.position, 
                              goal, agent.areaMask, path);
        // Set trajectories
        SetTrajectory(path);
        waypointIndex = 0;
        rotationNeeded = true;

        // For replanning
        elapsed = 0;
    }
    private void SetTrajectory(NavMeshPath path)
    {
        // Convert path into waypoints
        waypoints = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; ++i)
        {
            waypoints[i] = path.corners[i];
        }
        // Invalid Trajectory -> stop sign
        if (path.corners.Length == 0)
        {
            wheelController.SetRobotVelocity(0f, 0f);
        }
    }


    public void ClearGoal()
    {
        goal = new Vector3(0f, float.NegativeInfinity, 0f);
    }
    public void ClearPath()
    {
        path = new NavMeshPath();
        waypoints = new Vector3[0];
        SetTrajectory(path);
    }
}
