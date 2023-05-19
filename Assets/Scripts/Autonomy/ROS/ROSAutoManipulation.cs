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
    [SerializeField] private Transform mobileBaseTransform;
    [SerializeField] private ArticulationBody chest;

    // ROS communication
    [SerializeField] private PlanTrajectoryService planTrajectoryService;

    public override void PlanTrajectory(
        float[] currJointAngles,
        Vector3 targetPosition,
        Quaternion targetRotation,
        Action<float[], float[][], float[][], float[][]> callback,
        bool cartesianSpace = true
    )
    {
        /*
        // Joint Space planning is not suggested
        if (cartesianSpace == false)
        {
            Debug.Log(
                "Cartesian Space planning is not supported yet." +
                "Use joint space planning instead."
            );
        }
        */

        Debug.Log(targetPosition);

        // Offset the target position and orientation
        targetPosition = mobileBaseTransform.InverseTransformPoint(targetPosition);
        targetRotation = Quaternion.Inverse(targetRotation) * mobileBaseTransform.rotation;
        targetPosition.y -= chest.jointPosition[0];

        Debug.Log(targetPosition);

        // Send path planning request
        planTrajectoryService.SendPlanTrajectoryRequest(
            currJointAngles, targetPosition, targetRotation, callback
        );
    }
}
