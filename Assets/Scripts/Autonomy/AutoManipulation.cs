using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for arm manipulation.
///     It supports path planning and sequence of motion.
///     
///     This autonomy could be achived using
///     Simple trajectory planner with Newton IK (only for simulation robot) or
///     packages in ROS (feasible for both simulation and physical robot).
/// </summary>
public abstract class AutoManipulation : MonoBehaviour
{
    void Start() {}

    void Update() {}

    // Given current joint angles, target position and orientation,
    // Plan a trajectory to move the end effector along a trajectory defined by
    // a sequence of time-stamped joint angles, velocities and accelerations.
    public abstract void PlanTrajectory(
        float[] currJointAngles,
        Vector3 targetPosition,
        Quaternion targetRotation,
        Action<float[], float[][], float[][], float[][]> callback,
        bool cartesianSpace = false
    );
}
