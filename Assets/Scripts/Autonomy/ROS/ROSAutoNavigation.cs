using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for 2D navigation.
///
///     The global planner used is the default
///     Unity A* algorithm from NavMesh.
///     NavMesh is necessary for this planner.
///
///     The local planning strategy now is simply
///     "rotate then move forward"
/// </summary>
public class ROSAutoNavigation : AutoNavigation
{
    PoseStampedGoalPubisher publisher = new PoseStampedGoalPubisher();

    // Set goal, regardless of the goal orientation
    public override void SetGoal(Vector3 position)
    {
        SetGoal(position, new Vector3(0));
    }

    // Set goal, with goal orientation
    public override void SetGoal(Vector3 position, Vector3 orientation)
    {
        TargetPosition = position;
        TargetOrientationEuler = orientation;
    }

    //  Update Global Waypoints
    public void SetGlobalWaypoints(Vector3[] waypoints)
    {
        GlobalWaypoints = waypoints;
    }

    //  Update Local Waypoints
    public void SetLocalWaypoints(Vector3[] waypoints)
    {
        LocalWaypoints = waypoints;
    }

    // Start, pause and resume navigation
    // Start is essentially the same as resume
    public abstract void StartNavigation()
    {
        publisher.PublishPoseStampedCommand(TargetPosition, TargetOrientationEuler);
    }

    public abstract void PauseNavigation()
    {
        
    }
    public abstract void ResumeNavigation()
    {
        StartNavigation()
    }

    // Stop navigation, clear previous plan
    public abstract void StopNavigation();



}