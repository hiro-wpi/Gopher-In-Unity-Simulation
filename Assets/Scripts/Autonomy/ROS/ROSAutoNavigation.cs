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
    [SerializeField] private MoveBaseCancelGoalService cancelGoalService;
    [SerializeField] private MoveBaseSendGoalService sendGoalService;
    [SerializeField] private MoveBaseMakePlanService makePlanService;

    [SerializeField] private LocalPlannerSubscriber localPlanner;

    void Update()
    {
        // Update if there is a new global plan
        if(makePlanService.GetWaypointFlagStatus() == true)
        {
            GlobalWaypoints = makePlanService.GetGlobalWaypoints();
        }

        LocalWaypoints = localPlanner.getLocalWaypoints();
    }

    // Set goal, regardless of the goal orientation
    public override void SetGoal(Vector3 position)
    {
        SetGoal(position, new Vector3());
    }

    // Set goal, with goal orientation
    public override void SetGoal(Vector3 position, Vector3 orientation)
    {
        TargetPosition = position;
        TargetOrientationEuler = orientation;

        makePlanService.MakePlanCommandService(position, orientation);
    }

    
    // //  Update Local Waypoints
    // public void SetLocalWaypoints(Vector3[] waypoints)
    // {
    //     LocalWaypoints = waypoints;
    // }

    // Start, pause and resume navigation
    // Start is essentially the same as resume
    public override void StartNavigation()
    {
        // publisher.PublishPoseStampedCommand(TargetPosition, TargetOrientationEuler);
        sendGoalService.SendGoalCommandService(TargetPosition, TargetOrientationEuler);
    }

    public override void PauseNavigation()
    {
        cancelGoalService.CancelGoalCommandService();
    }
    public override void ResumeNavigation()
    {
        StartNavigation();
    }

    // Stop navigation, clear previous plan
    public override void StopNavigation()
    {
        cancelGoalService.CancelGoalCommandService();
        GlobalWaypoints = new Vector3[0];
        LocalWaypoints = new Vector3[0];
    }


}