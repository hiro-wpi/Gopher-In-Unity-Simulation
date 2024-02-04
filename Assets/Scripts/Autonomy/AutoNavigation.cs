using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for 2D navigation.
///     
///     This autonomy could be achived using
///     Unity NavMesh component (only for simulation robot)
///     or path planning packages (e.g. MoveBase) in ROS 
///     (feasible for both simulation and physical robot).
/// </summary>
public abstract class AutoNavigation : MonoBehaviour
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
    public bool IsNavigating { get; protected set; }
    [field:SerializeField, ReadOnly]
    public Vector3[] GlobalWaypoints { get; protected set; } = new Vector3[0];
    [field:SerializeField, ReadOnly]
    public Vector3[] LocalWaypoints { get; protected set; } = new Vector3[0];

    void Start() {}

    void Update() {}

    // Set goal
    public abstract void SetGoal(
        Vector3 position, Quaternion rotation, Action<Vector3[]> callback
    );

    // Start, pause and resume navigation
    // Start is essentially the same as resume
    public abstract void StartNavigation(Action baseReached = null);
    public abstract void PauseNavigation();
    public abstract void ResumeNavigation(Action baseReached = null);

    // Stop navigation, clear previous plan
    public abstract void StopNavigation();
}
