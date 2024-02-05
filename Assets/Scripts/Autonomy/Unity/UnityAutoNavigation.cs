using System;
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
    [SerializeField] private GameObject robot;
    [SerializeField] private ArticulationBaseController baseController;

    // Global planning
    // Nav mesh agent
    // this is not really used, only served as the parameter container
    // for speed, angularSpeed, stoppingDistance, areMask, etc.
    [SerializeField] private NavMeshAgent agent;
    private NavMeshObstacle[] obstacles;
    // Nav mesh planning
    private Coroutine planningCoroutine;
    private NavMeshPath path;

    // Local planning
    // start orientations
    [SerializeField] private PurePursuitPlanner purePursuitPlanner;
    [SerializeField] private float maxLinearSpeed = 0.75f;
    [SerializeField] private float maxAngularSpeed = 45f;
    private Quaternion startRotation;
    
    // Goal checked
    Action reachedAction = null;

    void Start()
    {
        // Never enabled, only used to specify parameters of nav mesh
        agent.enabled = false;

        // Get self nav mesh obstacles for global planner
        obstacles = robot.GetComponentsInChildren<NavMeshObstacle>();
        // Set robot for local planner
        purePursuitPlanner.SetRobots(robot);
    }

    void Update() {}

    void FixedUpdate()
    {
        // Goal not set or Autonomy paused
        if (!ValidGoalSet || !IsNavigating)
        {
            return;
        }

        // Move along the path
        var (linearSpeed, angularSpeed) = purePursuitPlanner.NextAction();
        Vector3 linearVelocity = new Vector3(
            0f, 0f, linearSpeed
        );
        Vector3 angularVelocity = new Vector3(
            0f, angularSpeed * Mathf.Deg2Rad, 0f
        );
        baseController.SetVelocity(linearVelocity, angularVelocity);

        // reached, stop
        if (purePursuitPlanner.IsGoalReached())
        {
            StopNavigation();
            Debug.Log("Done");
            reachedAction?.Invoke();
        }
    }

    // Set a new goal and try to plan a path to it
    public override void SetGoal(
        Vector3 position,
        Quaternion rotation = new Quaternion(),
        Action<Vector3[]> callback = null
    )
    {
        // Get closest point in the navmesh
        if (
            NavMesh.SamplePosition(
                position, out NavMeshHit hit, 1.0f, agent.areaMask
            )
        )
        {
            Vector3 goal = hit.position;
            if (planningCoroutine != null)
            {
                StopCoroutine(planningCoroutine);
            }
            planningCoroutine = StartCoroutine(
                PathPlanningCoroutine(goal, rotation, callback)
            );
        }
        else
        {
            Debug.Log("The given navigation goal is invalid.");
            callback?.Invoke(new Vector3[0]);
        }
    }

    private IEnumerator PathPlanningCoroutine(
        Vector3 goalPosition,
        Quaternion goalRotation,
        Action<Vector3[]> callback = null,
        float obstacleDisableTime = 0.5f
    )
    {
        SetObstacleActive(false);
        yield return new WaitForSeconds(0.1f);

        // path planning
        bool pathFound = PathPlanning(goalPosition, goalRotation);
        // path found or not
        if (pathFound)
        {
            ValidGoalSet = true;
            TargetPosition = goalPosition;
            TargetRotation = goalRotation;
            callback?.Invoke(GlobalWaypoints);
        }
        else
        {
            Debug.Log("No navigation path found to given navigation goal.");
            callback?.Invoke(new Vector3[0]);
        }

        yield return new WaitForSeconds(obstacleDisableTime);
        SetObstacleActive(true);
    }

    private void SetObstacleActive(bool active)
    {
        // In case arm is carrying objects
        foreach(NavMeshObstacle navMeshObstacle in obstacles)
        {
            navMeshObstacle.carving = active;
        }
    }

    private bool PathPlanning(Vector3 goalPosition, Quaternion goalRotation)
    {
        // Global Path finding - A*
        path = new NavMeshPath();
        NavMesh.CalculatePath(
            robot.transform.position, goalPosition, agent.areaMask, path
        );

        // Path not found
        if (path.corners.Length < 2)
        {
            return false;
        }

        // Set trajectories
        // Convert path into GlobalWaypoints
        GlobalWaypoints = new Vector3[path.corners.Length];
        for (int i = 0; i < path.corners.Length; ++i)
        {
            // position
            GlobalWaypoints[i] = path.corners[i];
        }

        // start rotation
        startRotation = Quaternion.LookRotation(
            path.corners[1] - path.corners[0]
        );

        // Init local controller parameters
        purePursuitPlanner.SetSpeedLimits(maxLinearSpeed, maxAngularSpeed);
        purePursuitPlanner.SetWaypoints(
            GlobalWaypoints, startRotation, goalRotation
        );

        return true;
    }

    // Start navigation
    public override void StartNavigation(Action baseReached = null)
    {
        ResumeNavigation(baseReached);
    }

    // Resume navigation
    public override void ResumeNavigation(Action baseReached = null)
    {
        // Must have valid goal and plan first
        if (!ValidGoalSet)
        {
            Debug.Log("No valid navigation goal set.");
            return;
        }

        IsNavigating = true;
        reachedAction = baseReached;
    }

    // Pause navigation
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
        TargetPosition = new Vector3();
        TargetRotation = new Quaternion();
        ValidGoalSet = false;

        path = new NavMeshPath();
        GlobalWaypoints = new Vector3[0];

        // Stop moving the robot
        baseController.SetVelocity(Vector3.zero, Vector3.zero);
        IsNavigating = false;
    }
}
