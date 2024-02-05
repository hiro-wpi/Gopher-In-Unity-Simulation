using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
///     The local path tracing algorithm pure pursuit
///     
///     This is just a simple implementation of the algorithm. 
///     It is not optimized and may not work in all cases.
/// </summary>
public class PurePursuitPlanner : MonoBehaviour
{
    // Robot
    [SerializeField] private GameObject robot;

    // Waypoints
    [SerializeField, ReadOnly] private Vector3[] waypoints;
    [SerializeField, ReadOnly] private int waypointIndex;
    private Quaternion startRotation;
    private Quaternion goalRotation;
    [SerializeField] private int numberOfWaypointInterpolation = 10;
    [SerializeField] private float lookAheadDistance = 0.5f;
    [SerializeField] private float positionTolerance = 0.2f;
    [SerializeField] private float rotationTolerance = 2f;

    // PD controller
    [SerializeField] private float Kp = 1.0f;
    [SerializeField] private float Kd = 0.1f;
    private float previousDistanceError = 0.0f;
    private float previousAngleError = 0.0f;
    private float maxLinearSpeed;
    private float maxAngularSpeed;

    void Start() {}

    void Update() {}

    public void SetRobots(GameObject robot)
    {
        this.robot = robot;
    }

    public void SetWaypoints(
        Vector3[] waypoints, Quaternion startRotation, Quaternion goalRotation
    )
    {
        // Initialize parameters
        waypointIndex = 0;
        previousDistanceError = 0.0f;
        previousAngleError = 0.0f;

        // Set up waypoints
        this.startRotation = startRotation;
        this.goalRotation = goalRotation;

        // Interpolate waypoints
        List<Vector3> interpolatedWaypoints = new List<Vector3>();
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            for (int j = 0; j < numberOfWaypointInterpolation; j++)
            {
                interpolatedWaypoints.Add(
                    Vector3.Lerp(
                        waypoints[i],
                        waypoints[i + 1],
                        (float) j / numberOfWaypointInterpolation
                    )
                );
            }
        }
        interpolatedWaypoints.Add(waypoints[waypoints.Length - 1]);
        this.waypoints = interpolatedWaypoints.ToArray();
    }

    public void SetSpeedLimits(float maxLinearSpeed, float maxAngularSpeed)
    {
        this.maxLinearSpeed = maxLinearSpeed;
        this.maxAngularSpeed = maxAngularSpeed;
    }

    public bool IsGoalReached()
    {
        return waypoints.Length != 0 && waypointIndex >= waypoints.Length;
    }

    public (float, float) NextAction()
    {
        // Goal reached
        if (waypointIndex >= waypoints.Length)
        {
            return (0f, 0f);
        }
        float linear = 0f;
        float angular = 0f;

        // Adjust the rotation at the beginning of the path
        if (waypointIndex == 0 && NeedStartRotationAdjustment())
        {
            // simple p controller with kp = 1
            angular = 1f * -Mathf.DeltaAngle(
                robot.transform.rotation.eulerAngles.y,
                startRotation.eulerAngles.y
            );
            return ClampSpeeds(0f, angular);
        }

        // Adjust the rotation at the end of the path
        if (EndPositionReached() && NeedEndRotationAdjustment())
        {
            // simple p controller with kp = 1
            angular = 1f * -Mathf.DeltaAngle(
                robot.transform.rotation.eulerAngles.y,
                goalRotation.eulerAngles.y
            );
            return ClampSpeeds(0f, angular);
        }

        // Goal reached
        else if (EndPositionReached() && !NeedEndRotationAdjustment())
        {
            waypointIndex = waypoints.Length;
            return (0f, 0f);
        }

        // Regular case
        (linear, angular) = PurePursuit();
        return ClampSpeeds(linear, angular);
    }

    private (float, float) PurePursuit()
    {
        // Get the current look ahead waypoint
        var (targetWaypoint, distance) = NextWaypoint();

        // Calculate the target in local space
        // transform the target waypoint into the robot's local space
        Vector3 localTarget = robot.transform.InverseTransformPoint(
            targetWaypoint
        );

        // PD control for rotation
        float curvature = 2 * -localTarget.x / (distance * distance);
        float turn = Kp * curvature + Kd * (curvature - previousAngleError);
        previousAngleError = curvature;

        // PD control for translation
        float speed = Kp * localTarget.z + Kd * (
            localTarget.z - previousDistanceError
        );
        previousDistanceError = distance;

        return (speed, turn * Mathf.Rad2Deg);
    }

    private (Vector3, float) NextWaypoint()
    {
        Vector3 targetWaypoint = waypoints[waypointIndex];
        float distance = Vector3.Distance(
            robot.transform.position, targetWaypoint
        );

        // Check if within lookAheadDistance 
        // to switch to the next waypoint
        while (
            distance < lookAheadDistance
            && waypointIndex < waypoints.Length - 1
        )
        {
            waypointIndex++;
            targetWaypoint = waypoints[waypointIndex];
            distance = Vector3.Distance(
                robot.transform.position, targetWaypoint
            );
        }

        return (targetWaypoint, distance);
    }

    private bool EndPositionReached()
    {
        return Vector3.Distance(
            robot.transform.position, waypoints[waypoints.Length - 1]
        ) < positionTolerance;
    }

    private bool NeedStartRotationAdjustment()
    {
        return Quaternion.Angle(
            robot.transform.rotation, startRotation
        ) > rotationTolerance;
    }

    private bool NeedEndRotationAdjustment()
    {
        // Invalid end rotation
        if (Quaternion.Dot(goalRotation, goalRotation) < Mathf.Epsilon)
        {
            return false;
        }

        // Valid end rotation
        return Quaternion.Angle(
            robot.transform.rotation, goalRotation
        ) > rotationTolerance;
    }

    private (float, float) ClampSpeeds(float linear, float angular)
    {
        return (
            Mathf.Clamp(linear, -maxLinearSpeed, maxLinearSpeed),
            Mathf.Clamp(angular, -maxAngularSpeed, maxAngularSpeed)
        );
    }
}
