using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for arm manipulation.
///     Plan a trajectory with ROS MoveIt
///     from current position to target position
/// </summary>
public class ROSAutoManipulation : AutoManipulation
{
    // Offset for the target position and orientation
    [SerializeField] private Transform baseTransform;

    // ROS communication
    [SerializeField] private PlanTrajectoryService planTrajectoryService;

    public override void PlanTrajectory(
        float[] currJointAngles,
        Vector3 targetPosition,
        Quaternion targetRotation,
        Action<float[], float[][], float[][], float[][]> callback,
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

        // Offset the target position and orientation
        targetPosition = baseTransform.InverseTransformPoint(targetPosition);
        targetRotation = Quaternion.Inverse(baseTransform.rotation) * targetRotation;

        // Send path planning request
        planTrajectoryService.SendPlanTrajectoryRequest(
            currJointAngles, targetPosition, targetRotation, callback
        );
    }
}
