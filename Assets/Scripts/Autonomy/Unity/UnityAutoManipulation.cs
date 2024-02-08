using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for arm manipulation.
///     
///     Plan a simple straight-line trajectory
///     from current position to target position
/// </summary>
public class UnityAutoManipulation : AutoManipulation
{
    // Arm Controller
    [SerializeField] private ArticulationArmController armController;
    [SerializeField] private ArticulationJointController jointController;

    // Planner
    [SerializeField] private StraightLinePlanner planner;
    private Action<float[], float[][], float[][], float[][]> otherCallback;

    void Start() {}

    void Update() {}

    public override void PlanTrajectory(
        float[] currJointAngles,
        Vector3 targetPosition,
        Quaternion targetRotation,
        Action<float[], float[][], float[][], float[][]> callback,
        bool cartesianSpace = true
    )
    {
        otherCallback = callback;
        planner.PlanTrajectory(
            currJointAngles,
            targetPosition,
            targetRotation,
            TrajectoryCallback,
            cartesianSpace
        );
    }

    private void TrajectoryCallback(
        float[] timeSteps,
        float[][] jointAngles,
        float[][] jointVelocities,
        float[][] jointAccelerations
    )
    {
        // Check validity of the path
        if (timeSteps == null || timeSteps.Length <= 1)
        {
            ValidGoalSet = false;
            otherCallback(
                timeSteps,
                jointAngles, 
                jointVelocities,
                jointAccelerations
            );

            Debug.Log("No manipulation path found");
            return;
        }

        // Set the trajectory
        ValidGoalSet = true;
        TimeSteps = timeSteps;
        Angles = jointAngles;
        Velocities = jointVelocities;
        Accelerations = jointAccelerations;

        otherCallback(
            TimeSteps,
            Angles, 
            Velocities,
            Accelerations
        );
    }

    // Start manipulation
    public override void StartManipulation(Action armReached = null)
    {
        // Must have valid goal and plan first
        if (!ValidGoalSet)
        {
            Debug.Log("No valid manipulation goal set.");
            return;
        }

        IsManipulating = true;
        if (!IsManipulating)
        {
            jointController.SetJointTrajectory(
                TimeSteps,
                Angles, 
                Velocities,
                Accelerations,
                armReached
            );
        }
    }

    // Resume manipulation
    public override void ResumeManipulation(Action armReached = null)
    {
        throw new NotImplementedException();
    }

    // Pause manipulation
    public override void PauseManipulation()
    {
        throw new NotImplementedException();
    }

    // Terminate manipulation
    public override void StopManipulation()
    {
        // Init parameters
        TargetPosition = new Vector3();
        TargetRotation = new Quaternion();
        ValidGoalSet = false;

        // Clear plan
        TimeSteps = new float[0];
        Angles = new float[0][];
        Velocities = new float[0][];
        Accelerations = new float[0][];

        // Stop moving the arm
        jointController.StopJoints();
        IsManipulating = false;
    }
}
