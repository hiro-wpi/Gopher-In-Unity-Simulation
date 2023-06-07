﻿using System.Collections;
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
    // Move base services
    [SerializeField] private MoveBaseCancelGoalService cancelGoalService;
    [SerializeField] private MoveBaseSendGoalService sendGoalService;

    // Local and global planners
    [SerializeField] private PathSubscriber pathPlanner;

    // AMCL
    [SerializeField] private PoseWithCovarianceStampedPublisher poseWithCovarianceStampedPublisher;

    [SerializeField] private TwistSubscriber twistSubscriber;
    [SerializeField] private PolygonPublisher polygonPublisher;

    [SerializeField] private GameObject robot;

    // Flags
    private bool isTwistSubscriberPaused = false;
    private bool isInitcialPosePublished = false;



    private bool updateWaypoints = true;

    void Start()
    {   
        StartCoroutine(PauseTwistSubscriber());
        StartCoroutine(PublishInitcialPose());
    }

    // Pauses the Twist Subscriber from controlling the base given Twist Commands
    IEnumerator PauseTwistSubscriber()
    {
        yield return new WaitForFixedUpdate();

        // Pause Twist Subscriber
        twistSubscriber.Pause(true);
        isTwistSubscriberPaused = true;
    }

    // Publishes the Initcial pose of the robot to AMCL
    IEnumerator PublishInitcialPose()
    {
        yield return new WaitForFixedUpdate();

        // Send the initcial Pose
        poseWithCovarianceStampedPublisher.PublishPoseStampedCommand(robot.transform.position, robot.transform.rotation.eulerAngles);
        isInitcialPosePublished = true;
    }


    void Update()
    {
        
        // Initcialize twist subsciber
        if(isTwistSubscriberPaused == false)
        {
            return;
        }
        
        // Publish initial pose
        if(isInitcialPosePublished == false)
        {
            return;
        }

        if(updateWaypoints)
        {
            GlobalWaypoints = pathPlanner.getGlobalWaypoints();
            LocalWaypoints = pathPlanner.getLocalWaypoints();
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
        UpdateNav(false);
        updateWaypoints = true;
    }

    // Start, pause and resume navigation
    // Start is essentially the same as resume
        // Allow the robot to move along the planned waypoints
    public override void StartNavigation()
    {
        UpdateNav(true);
        updateWaypoints = true;
    }

    // Pause robot navigation
        // Stop the motion of the robot. Still allow the waypoints to be seen
    public override void PauseNavigation()
    {
        UpdateNav(false);
        updateWaypoints = true;
    }

    // Resume navigation
    public override void ResumeNavigation()
    {
        UpdateNav(true);
        updateWaypoints = true;
    }

    // Stop navigation, clear previous plan
    public override void StopNavigation()
    {
        cancelGoalService.CancelGoalCommand();
        GlobalWaypoints = new Vector3[0];
        LocalWaypoints = new Vector3[0];
        UpdateNav(false);
        updateWaypoints = false;
    }

    // Update the navigation status of the robot, and pause listening to the twist subsriber
    private void UpdateNav(bool isNav)
    {
        IsNavigating = isNav;
        twistSubscriber.Pause(!isNav);
    }

    // Update the footprint of the robot used in the navigation stack by the DWA local planner
    public void UpdateFootprint(Vector3[] poly)
    {
        polygonPublisher.PublishPolygon(poly);
    }

    public void SetToNormalFootprint()
    {   /*** 
            WRT ROS it is:
            
            footprint:  [-0.2f, -0.32f, 0.0f]
                        [-0.2f,  0.32f, 0.0f]
                        [ 0.3f,  0.32f, 0.0f]
                        [ 0.3f, -0.32f, 0.0f]

            WRT Unity it should be:
        
            footprint:  [ 0.32f,  0.0f, -0.2f]
                        [-0.32f,  0.0f, -0.2f]
                        [-0.32f,  0.0f,  0.3f]
                        [ 0.32f,  0.0f,  0.3f]
        
        ***/
        Vector3[] polygon = {   new Vector3( 0.32f,  0.0f, -0.2f),
                                new Vector3(-0.32f,  0.0f, -0.2f),
                                new Vector3(-0.32f,  0.0f,  0.3f),
                                new Vector3( 0.32f,  0.0f,  0.3f)};

        UpdateFootprint(polygon);

    }

    public void SetToBaseWithCartFootprint()
    {   /*** 
            WRT ROS it is:
            
            footprint:  [-0.2f, -0.32f, 0.0f]
                        [-0.2f,  0.32f, 0.0f]
                        [ 1.5f,  0.32f, 0.0f]
                        [ 1.5f, -0.32f, 0.0f]

            WRT Unity it should be:
        
            footprint:  [ 0.32f,  0.0f, -0.2f]
                        [-0.32f,  0.0f, -0.2f]
                        [-0.32f,  0.0f,  1.5f]
                        [ 0.32f,  0.0f,  1.5f]
        
        ***/
        Vector3[] polygon = {   new Vector3( 0.32f,  0.0f, -0.2f),
                                new Vector3(-0.32f,  0.0f, -0.2f),
                                new Vector3(-0.32f,  0.0f,  1.5f),
                                new Vector3( 0.32f,  0.0f,  1.5f)};

        UpdateFootprint(polygon);

    }
}
