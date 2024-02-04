using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Plan a simple straight-line trajectory
///     from current position to target position
/// </summary>
public class StraightLinePlanner : MonoBehaviour
{
    // Parameter
    [SerializeField] private int numberOfWaypoints = 5;
    // For planning in cartesian space
    [SerializeField] private float cartesianSpeed = 0.05f;
    // For planning in joint space
    [SerializeField] private float jointSpeed = 0.5f;
    private float completionTime;

    // Kinematic solver
    [SerializeField] private ForwardKinematics forwardKinematics;
    [SerializeField] private InverseKinematics inverseKinematics;

    public void PlanTrajectory(
        float[] currJointAngles,
        Vector3 targetPosition,
        Quaternion targetRotation,
        Action<float[], float[][], float[][], float[][]> callback,
        bool cartesianSpace = true
    )
    {
        float[] timeSteps;
        float[][] angles;
        float[][] velocities;
        float[][] accelerations;

        // Cartesian Space planning using a simple straight line
        if (cartesianSpace == true)
        {
            (timeSteps, angles, velocities, accelerations) = 
                PlanStraightLine(
                    currJointAngles,
                    targetPosition,
                    targetRotation
                );
        }

        // Not recommended to use
        // Simple interpolation between current and target joint angles
        else
        {
            // Debug.Log(
            //     "Not recommended to use planning in joint space. "
            //     + "Plan in cartesian space instead. "
            //     + "Try to set cartesianSpace to True."
            // );
            // Will still use straght line planning in cartesian space instead
            (timeSteps, angles, velocities, accelerations) = 
                PlanStraightLine(
                    currJointAngles,
                    targetPosition,
                    targetRotation
                );
            // (timeSteps, angles, velocities, accelerations) = 
            //     PlanJointInterpolation(
            //         currJointAngles,
            //         targetPosition,
            //         targetRotation
            //     );
        }

        // Send the result back to the caller
        callback(timeSteps, angles, velocities, accelerations);
    }

    private (float[], float[][], float[][], float[][]) PlanStraightLine(
        float[] currJointAngles,
        Vector3 targetPosition,
        Quaternion targetRotation
    )
    {
        // Initialize 
        float[] timeSteps = new float[numberOfWaypoints];
        float[][] angles = new float[numberOfWaypoints][];
        // velocities and accelerations are not used
        float[][] velocities = new float[numberOfWaypoints][];
        float[][] accelerations = new float[numberOfWaypoints][];

        // Compute current position and rotation
        forwardKinematics.SolveFK(currJointAngles);
        var (startPosition, startRotation) = forwardKinematics.GetPose(
            forwardKinematics.NumJoint
        );

        // Calculate time step
        completionTime = (
            Vector3.Distance(startPosition, targetPosition)
            + Quaternion.Angle(
                startRotation, targetRotation
            ) * Mathf.Deg2Rad / 10  // 4 is a scaler
        ) / cartesianSpeed;

        // Interpolate between Start and Goal (positions and rotations)
        Vector3[] waypointsPositions = InterpolatePosition(
            startPosition, targetPosition, numberOfWaypoints
        );
        Quaternion[] waypointsRotations = InterpolateRotation(
            startRotation, targetRotation, numberOfWaypoints
        );

        // First waypoint is current position
        timeSteps[0] = 0;
        angles[0] = currJointAngles;

        // Solve IK for each waypoint
        for (int i = 1; i < numberOfWaypoints; ++i)
        {
            // time
            timeSteps[i] = completionTime * i / (numberOfWaypoints - 1);

            // Solve to get new Joint Angles
            currJointAngles = inverseKinematics.SolveIK(
                currJointAngles, waypointsPositions[i], waypointsRotations[i]
            );
            angles[i] = currJointAngles;
        }

        // Check if this is a valid solution
        bool converged = CheckConfiguration(
            angles[angles.Length - 1], targetPosition, targetRotation
        );
        if (!converged)
        {
            Debug.Log("No valid path to the given target.");
            return (
                new float[0], new float[0][], new float[0][], new float[0][]
            );
        }

        return (timeSteps, angles, velocities, accelerations);
    }

    // Interpolation function
    private Vector3[] InterpolatePosition(Vector3 start, Vector3 goal, int num)
    {
        Vector3[] positions = new Vector3[num];
        for (int i = 0; i < num; i++)
        {
            float t = i / (float) (num - 1);
            positions[i] = Vector3.Lerp(start, goal, t);
        }
        return positions;
    }

    private Quaternion[] InterpolateRotation(
        Quaternion start, Quaternion goal, int num
    ) 
    {
        Quaternion[] rotations = new Quaternion[num];
        for (int i = 0; i < num; i++)
        {
            float t = i / (float) (num - 1);
            rotations[i] = Quaternion.Slerp(start, goal, t);
        }
        return rotations;
    }

    public bool CheckConfiguration(
        float[] jointAngles, Vector3 targetPosition, Quaternion targetRotation
    ) 
    {
        // calculate error between our current 
        // end effector position and the target position
        forwardKinematics.SolveFK(jointAngles, updateJacobian: true);
        var (eePosition, eeRotation) = forwardKinematics.GetPose(
            forwardKinematics.NumJoint
        );

        float positionError = Vector3.Distance(eePosition, targetPosition);
        float rotationError = Quaternion.Angle(
            eeRotation, targetRotation
        ) * Mathf.Deg2Rad;

        // Check convergence
        return positionError < 1e-3 && rotationError < 2e-2;
    }

    // Plan trajectory in joint space with simple interpolation
    private (float[], float[][], float[][], float[][]) PlanJointInterpolation(
        float[] currJointAngles,
        Vector3 targetPosition,
        Quaternion targetRotation
    )
    {
        // Initialize 
        int numJoints = currJointAngles.Length;
        float[] timeSteps = new float[numJoints];
        float[][] angles = new float[numJoints][];
        // velocities and accelerations are not used
        float[][] velocities = new float[numJoints][];
        float[][] accelerations = new float[numJoints][];

        // Check if there is a solution
        var targetJointAngles = inverseKinematics.SolveIK(
            currJointAngles, targetPosition, targetRotation
        );
        bool converged = CheckConfiguration(
            targetJointAngles, targetPosition, targetRotation
        );
        if (!converged)
        {
            Debug.Log("No valid path to the given target.");
            return (
                new float[0], new float[0][], new float[0][], new float[0][]
            );
        }

        // Lerp between points to generate a path
        completionTime = MaxDifferent(
            currJointAngles, targetJointAngles
        ) / jointSpeed;

        // First waypoint is current position
        timeSteps[0] = 0;
        angles[0] = currJointAngles;

        // Lerp joint angles between current and target
        for (int i = 1; i < numberOfWaypoints; ++i)
        {
            // time
            timeSteps[i] = completionTime * i / (numberOfWaypoints - 1);

            // angles
            float[] jointValues = new float[numJoints];
            for (int j = 0; j < numJoints; j++)
            {
                jointValues[j] = Mathf.Lerp(
                    currJointAngles[j],
                    targetJointAngles[j],
                    i / (float) (numberOfWaypoints - 1)
                );
            }
            angles[i] = jointValues;
        }

        return (timeSteps, angles, velocities, accelerations);
    }

    private float MaxDifferent(float[] ang1, float[] ang2)
    {
        float maxDiff = 0;
        for (var i = 0; i < ang1.Length; i++)
        {
            var diff = Mathf.Abs(ang1[i] - ang2[i]);
            if (diff > maxDiff)
            {
                maxDiff = diff;
            }
        }
        return maxDiff;
    }
}
