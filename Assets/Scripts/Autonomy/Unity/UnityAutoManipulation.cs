using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for arm manipulation.
///     Plan a simple straight-line trajectory 
///     from current position to target position
/// </summary>
public class UnityAutoManipulation : AutoManipulation
{
    // Parameter
    // [SerializeField] private float cartesianSpeed = 0.05f;
    [SerializeField] private float jointSpeed = 0.5f;
    [SerializeField] private int numberOfWaypoints = 3;
    private float completionTime;

    // Kinematic solver
    public ForwardKinematics forwardKinematics;
    public InverseKinematics inverseKinematics;

    void Start() {}

    void Update() {}

    public override (float[], float[][], float[][], float[][]) PlanTrajectory(
        float[] currJointAngles, 
        Vector3 targetPosition, 
        Quaternion targetRotation,
        bool cartesianSpace = false
    )
    {
        // Cartesian Space planning is not supported yet
        if (cartesianSpace == true)
        {
            Debug.Log(
                "Cartesian Space planning is not supported yet." +
                "Use joint space planning instead."
            );
        }
        
        // Solve IK for target position joint angles
        var (converged, targetJointAngles) = inverseKinematics.SolveIK(
            currJointAngles, targetPosition, targetRotation
        );
        if (!converged)
        {
            Debug.Log("No valid path to given target.");
            return (null, null, null, null);
        }

        // Lerp between points to generate a path
        completionTime = GetMaxDifferent(
            currJointAngles, targetJointAngles
        ) / jointSpeed;

        // Get trajectory
        return GenerateJointTrajectory(
            currJointAngles,
            targetJointAngles,
            numberOfWaypoints,
            completionTime
        );
    }

    private float GetMaxDifferent(float[] ang1, float[] ang2)
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

    // Generate a simple joint trajectory without velocity and acceleration
    private (float[], float[][], float[][], float[][]) GenerateJointTrajectory(
        float[] currentAngles,
        float[] targetAngles,
        int numWaypoints,
        float completionTime
    )
    {
        // Initialize containers
        int numJoints = currentAngles.Length;
        float[] times = new float[numWaypoints+1];
        float[][] angles = new float[numWaypoints+1][];
        // float[][] velocities = new float[numWaypoints][];
        // float[][] accelerations = new float[numWaypoints][];

        // First waypoint is current position
        times[0] = 0;
        angles[0] = currentAngles;

        // Lerp joint angles between current and target
        for (int t = 1; t < numWaypoints+1; ++t)
        {
            // time
            times[t] = t / (float)numWaypoints * completionTime;
            
            // angles
            float[] jointValues = new float[numJoints];
            for (int i = 0; i < numJoints; i++)
            {
                jointValues[i] = Mathf.Lerp(
                    currentAngles[i],
                    targetAngles[i],
                    t / (float)numWaypoints
                );
            }
            angles[t] = jointValues;
        }

        // Return trajectory without velocity and acceleration
        return (times, angles, null, null);
    }
}
