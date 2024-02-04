using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for arm manipulation.
///     It supports path planning and sequence of motion.
///     
///     This autonomy could be achived using
///     Simple straight-line trajectory planner with Jacobian IK 
///     (only for simulation robot) 
///     or motion planning packages (e.g. MoveIt!) in ROS
///     (feasible for both simulation and physical robot).
/// </summary>
public abstract class AutoManipulation : MonoBehaviour
{
    // Goal
    [field:SerializeField, ReadOnly]
    public bool ValidGoalSet { get; protected set; }
    [field:SerializeField, ReadOnly]
    public Vector3 TargetPosition { get; protected set; }
    [field:SerializeField, ReadOnly]
    public Quaternion TargetRotation { get; protected set; }

    // Navigation status
    [field:SerializeField, ReadOnly]
    public bool IsManipulating { get; protected set; }
    // [field:SerializeField, ReadOnly]
    public float[] TimeSteps { get; protected set; } = new float[0];
    // [field:SerializeField, ReadOnly]
    public float[][] Angles { get; protected set; } = new float[0][];
    // [field:SerializeField, ReadOnly]
    public float[][] Velocities { get; protected set; } = new float[0][];
    // [field:SerializeField, ReadOnly]
    public float[][] Accelerations { get; protected set; } = new float[0][];

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

    // Start, pause and resume manipulation
    // Start is essentially the same as resume
    public abstract void StartManipulation(Action armReached = null);
    public abstract void PauseManipulation();
    public abstract void ResumeManipulation(Action armReached = null);

    // Stop Manipulation, clear previous plan
    public abstract void StopManipulation();
}
