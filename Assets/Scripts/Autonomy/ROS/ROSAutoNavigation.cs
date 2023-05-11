using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Autonomy for 2D navigation.
///
///     The global planner references the 
///     navigation_stack for global planning.
///
///     The local planning uses dwa or teb local planners
/// </summary>
public class ROSAutoNavigation : AutoNavigation
{
    [SerializeField] private MoveBaseCancelGoalService cancelGoalService;
    [SerializeField] private MoveBaseSendGoalService sendGoalService;
    [SerializeField] private MoveBaseMakePlanService makePlanService;

    [SerializeField] private LocalPlannerSubscriber localPlanner;
    [SerializeField] private GlobalPlannerSubscriber globalPlanner;

    [SerializeField] private TwistSubscriber twistSubscriber;

    private bool updateWaypoints = true;

    void Start()
    {
        twistSubscriber.Pause(true);
    }


    void Update()
    {

        if(updateWaypoints)
        {
            GlobalWaypoints = globalPlanner.getGlobalWaypoints();
            LocalWaypoints = localPlanner.getLocalWaypoints();
        }
        
    }    

    // Set goal, regardless of the goal orientation
    public override void SetGoal(Vector3 position)
    {
        SetGoal(position, new Vector3(0,0,0));
    }

    // Set goal, with goal orientation
        // Local and global waypoints are displayed based on the sent goal
        // The robot doesn't move 
    public override void SetGoal(Vector3 position, Vector3 orientation)
    {
        TargetPosition = position;
        TargetOrientationEuler = orientation;

        sendGoalService.SendGoalCommandService(position, orientation);
        updateNav(false);
        updateWaypoints = true;
    }

    // Start, pause and resume navigation
    // Start is essentially the same as resume
        // Allow the robot to move along the planned waypoints
    public override void StartNavigation()
    {
        updateNav(true);
        updateWaypoints = true;
    }

    // Pause robot navigation
        // Stop the motion of the robot. Still allow the waypoints to be seen
    public override void PauseNavigation()
    {
        updateNav(false);
        updateWaypoints = true;
    }

    // Resume navigation
    public override void ResumeNavigation()
    {
        updateNav(true);
        updateWaypoints = true;
    }

    // Stop navigation, clear previous plan
    public override void StopNavigation()
    {
        cancelGoalService.CancelGoalCommandService();
        GlobalWaypoints = new Vector3[0];
        LocalWaypoints = new Vector3[0];
        updateNav(false);
        updateWaypoints = false;
    }

    // Update the navigation status of the robot, and pause listening to the twist subsriber
    private void updateNav(bool isNav)
    {
        IsNavigating = isNav;
        twistSubscriber.Pause(!isNav);
    }


}