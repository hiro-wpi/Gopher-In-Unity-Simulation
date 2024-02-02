using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private 
        PoseWithCovarianceStampedPublisher poseWithCovarianceStampedPublisher;

    [SerializeField] private TwistSubscriber twistSubscriber;

    [SerializeField] private GameObject robot;

    // Flags
    private bool isTwistSubscriberPaused = false;
    private bool isInitialPosePublished = false;

    private bool updateWaypoints = true;

    void Start()
    {   
        StartCoroutine(PauseTwistSubscriber());
        StartCoroutine(PublishInitialPose());
    }

    // Pauses the Twist Subscriber from controlling the base
    IEnumerator PauseTwistSubscriber()
    {
        yield return new WaitForFixedUpdate();

        // Pause Twist Subscriber
        twistSubscriber.enabled = true;
        isTwistSubscriberPaused = true;
    }

    // Publishes the Initial pose of the robot to AMCL
    IEnumerator PublishInitialPose()
    {
        yield return new WaitForFixedUpdate();

        // Send the Initial Pose
        poseWithCovarianceStampedPublisher.PublishPoseStampedCommand(
            robot.transform.position, robot.transform.rotation.eulerAngles
        );
        isInitialPosePublished = true;
    }

    // TEMP TO BE REMOVED
    public void OnResumeNavigation(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ResumeNavigation();
        }
    }

    public void OnPauseNavigation(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PauseNavigation();
        }
    }

    public void OnStopNavigation(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            StopNavigation();
        }
    }

    void Update()
    {
        
        // Initialize twist subsciber
        if(isTwistSubscriberPaused == false)
        {
            return;
        }
        
        // Publish initial pose
        if(isInitialPosePublished == false)
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
        SetGoal(position, new Quaternion());
    }

    // Set goal, with goal orientation
        // Local and global waypoints are displayed based on the sent goal
        // The robot doesn't move 
    public override void SetGoal(Vector3 position, Quaternion rotation)
    {
        TargetPosition = position;
        TargetOrientationEuler = rotation.eulerAngles;

        sendGoalService.SendGoalCommandService(
            TargetPosition, TargetOrientationEuler
        );
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
    // Stop the motion of the robot. 
    // Still allow the waypoints to be seen
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

    // TEMP
    // Update the navigation status of the robot, 
    // and pause listening to the twist subsriber
    private void UpdateNav(bool isNav)
    {
        IsNavigating = isNav;
        twistSubscriber.enabled = isNav;
    }
}
