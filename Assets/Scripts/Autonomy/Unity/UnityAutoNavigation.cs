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
    // Waypoint orientations
    [SerializeField, ReadOnly] private Quaternion[] waypointRotations;

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
    Action reachedAction = null;

    // Local planning
    // replan the trajectory every x seconds
    [SerializeField] private float replanTime = 5f;
    private float elapsed;
    // waypoint checking
    private int waypointIndex;
    [SerializeField] private float positionTolerance = 0.2f;
    [SerializeField] private float rotationTolerance = 2f;
    private bool rotationAdjustment = true;
    // local controller
    [SerializeField] private float Kp = 1;
    [SerializeField] private float Kd = 0.1f;
    private float previousDistanceError = 0.0f;
    private float previousAngleError = 0.0f;

    void Start()
    {
        // Never enabled
        agent.enabled = false;
        // Get self nav mesh obstacles
        obstacles = robot.GetComponentsInChildren<NavMeshObstacle>();
    }

    void Update() {}

    void FixedUpdate()
    {
        // Goal not set or Autonomy paused
        if (!ValidGoalSet || !IsNavigating)
        {
            return;
        }

        // Replan check
        elapsed += Time.fixedDeltaTime;
        if (elapsed > replanTime)
        {
            elapsed = 0f;
            SetGoal(TargetPosition, TargetRotation);
        }

        // Move along the path
        FollowTrajectory();
    }

    private void FollowTrajectory()
    {
        // Select tolerance
        if (waypointIndex == GlobalWaypoints.Length - 1)
        {
            positionTolerance = agent.stoppingDistance;
        }
        else
        {
            positionTolerance = 0.1f;
        }
        // Check waypoint reached
        float distanceError = (
            GlobalWaypoints[waypointIndex] - robot.transform.position
        ).magnitude;
        float angleError = Mathf.Abs(
            waypointRotations[waypointIndex].eulerAngles.y 
            - robot.transform.rotation.eulerAngles.y
        );

        // Not reached, move to the waypoint
        if (
            distanceError > positionTolerance 
            || angleError > rotationTolerance
        )
        {
            NavigateToNextWaypoint();
        }

        // Current waypoint reached, next
        else
        {
            // fianl goal is reached
            if (waypointIndex == GlobalWaypoints.Length - 1)
            {
                reachedAction?.Invoke();
                StopNavigation();
            }
            // next waypoint
            else
            {
                waypointIndex++;
                rotationAdjustment = true;
            }
        }
    }

    // Implementa a simple strategy and local controller to move the robot
    private void NavigateToNextWaypoint()
    {
        // This should not happen
        if (waypointIndex >= GlobalWaypoints.Length)
        {
            return;
        }

        Vector3 position = GlobalWaypoints[waypointIndex];
        Quaternion rotation = waypointRotations[waypointIndex]; 

        // Errors
        float distanceError = (position - robot.transform.position).magnitude;
        float angleError = (
            rotation.eulerAngles.y - robot.transform.rotation.eulerAngles.y
        );

        // Adjust rotation angle first
        if (
            Mathf.Abs(angleError) > rotationTolerance
            && rotationAdjustment
        )
        {
            // PD controller
            float deltaAngleError = angleError - previousAngleError;
            previousAngleError = angleError;

            float angularSpeed = - (Kp * angleError + Kd * deltaAngleError);
            angularSpeed = Mathf.Clamp(
                angularSpeed, -agent.angularSpeed, agent.angularSpeed
            );

            // Set angular velocity
            Vector3 angularVelocity = new Vector3(
                0f, angularSpeed * Mathf.Deg2Rad, 0f
            );
            baseController.SetVelocity(Vector3.zero, angularVelocity);
        }
        // Then handle by local planner
        else
        {
            rotationAdjustment = false;

            // PD controller
            float deltaDistanceError = distanceError - previousDistanceError;
            previousDistanceError = distanceError;
            
            float linearSpeed = Kp * distanceError + Kd * deltaDistanceError;

            // Set linear velocity
            linearSpeed = Mathf.Clamp(linearSpeed, -agent.speed, agent.speed);
            // set linear speed
            Vector3 linearVelocity = new Vector3(0f, 0f, linearSpeed);
            baseController.SetVelocity(linearVelocity, Vector3.zero);
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
            Debug.Log("The given goal is invalid.");
            callback?.Invoke(new Vector3[0]);
        }
    }

    private IEnumerator PathPlanningCoroutine(
        Vector3 goal,
        Quaternion endRotation,
        Action<Vector3[]> callback = null,
        float obstacleDisableTime = 0.5f
    )
    {
        SetObstacleActive(false);
        yield return new WaitForSeconds(0.1f);

        // path planning
        bool pathFound = PathPlanning(goal, endRotation);
        // path found or not
        if (pathFound)
        {
            ValidGoalSet = true;
            TargetPosition = goal;
            TargetRotation = endRotation;
            callback?.Invoke(GlobalWaypoints);
        }
        else
        {
            Debug.Log("No path found to given goal.");
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

    private bool PathPlanning(Vector3 endPosition, Quaternion endRotation)
    {
        // Global Path finding - A*
        path = new NavMeshPath();
        NavMesh.CalculatePath(
            robot.transform.position, endPosition, agent.areaMask, path
        );

        // Path not found
        if (path.corners.Length < 2)
        {
            return false;
        }

        // Set trajectories
        // If end rotation set, add one more waypoint to store the end rotation
        int pathLength = path.corners.Length;
        if (endRotation != new Quaternion())
        {
            pathLength += 1;
        }

        // Convert path into GlobalWaypoints
        GlobalWaypoints = new Vector3[pathLength];
        waypointRotations = new Quaternion[pathLength];
        for (int i = 0; i < path.corners.Length; ++i)
        {
            GlobalWaypoints[i] = path.corners[i];
            if (i != 0)
            {
                waypointRotations[i] = Quaternion.LookRotation(
                    path.corners[i] - path.corners[i - 1]
                );
            }
            else
            {
                waypointRotations[i] = waypointRotations[i + 1];
            }
        }

        // As mentioned, add the end rotation
        if (endRotation != new Quaternion())
        {
            GlobalWaypoints[path.corners.Length] = 
                GlobalWaypoints[path.corners.Length - 1];
            waypointRotations[path.corners.Length] = endRotation;
        }

        // Init local controller parameters
        waypointIndex = 0;
        rotationAdjustment = true;
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
            Debug.Log("No valid goal is set.");
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
        waypointRotations = new Quaternion[0];

        // Stop moving the robot
        baseController.SetVelocity(Vector3.zero, Vector3.zero);
        IsNavigating = false;
    }
}
