using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script send Unity input for camera control 
///     to ROS as Twist message.
///
///     The current velocity is published to ROS at a fixed rate.
/// </summary>
public class PhysicalCameraController : CameraController
{
    // ROS communication
    [SerializeField] private TwistPublisher twistPublisher;

    // Velocity publish rate
    [SerializeField] protected int publishRate = 60;

    void Start()
    {
        // Keep publishing the velocity at a fixed rate
        InvokeRepeating("PublishVelocity", 1.0f, 1.0f / publishRate);
    }

    void Update() {}

    // Publish the current velocity
    private void PublishVelocity()
    {
        if (controlMode != ControlMode.Speed) 
        {
            return;
        }
        
        // Publish to ROS
        twistPublisher.PublishTwist(new Vector3(0,0,0), angularVelocity);
    }

    public override void StopCamera()
    {
        twistPublisher.PublishTwist(Vector3.zero, Vector3.zero);
    }

    // TODO
    public override void HomeCamera()
    {
        
    }
}
