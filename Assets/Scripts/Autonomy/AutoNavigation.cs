using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for 2D navigation.
///     
///     This autonomy could be achived using
///     Unity NavMesh component (only for simulation robot) or
///     packages in ROS (feasible for both simulation and physical robot).
/// </summary>
public abstract class AutoNavigation : MonoBehaviour
{
    // Goal
    [field:SerializeField, ReadOnly]
    public Vector3 TargetPosition { get; protected set; }
    [field:SerializeField, ReadOnly]
    public Vector3 TargetOrientationEuler { get; protected set; }

    // Navigation status
    [field:SerializeField, ReadOnly]
    public bool IsNavigating { get; protected set; }
    [field:SerializeField, ReadOnly]
    public Vector3[] GlobalWaypoints { get; protected set; }
    [field:SerializeField, ReadOnly]
    public Vector3[] LocalWaypoints { get; protected set; }

    void Start() 
    {
        GlobalWaypoints = new Vector3[0];
        LocalWaypoints = new Vector3[0];
    }

    void Update() {}

    // Set goal, regardless of the goal orientation
    public abstract void SetGoal(Vector3 position);
    // Set goal, with goal orientation
    public abstract void SetGoal(Vector3 position, Vector3 orientation);

    // Start, pause and resume navigation
    // Start is essentially the same as resume
    public abstract void StartNavigation();
    public abstract void PauseNavigation();
    public abstract void ResumeNavigation();

    // Stop navigation, clear previous plan
    public abstract void StopNavigation();
}
