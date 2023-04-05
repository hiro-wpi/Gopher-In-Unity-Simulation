using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     This script send Unity input for base control 
///     to ROS as Twist message.
///
///     Three control modes are available: Slow, Regular, and Fast,
///     which correspond to 0.5, 1.0 of the max velocity.
///     Clipping and smoothing are also applied to the input.
///     
///     The current velocity is published to ROS at a fixed rate.
/// </summary>
public class PhysicalWheelController : WheelController
{
    // ROS communication
    [SerializeField] private TwistPublisher twistPublisher;

    [SerializeField] private float publishRate = 60f;
    [SerializeField] private float publishDeltaTime;
    
    protected override void Start()
    {
        // Update velocity
        base.Start();

        // Keep publishing the velocity at a fixed rate
        publishDeltaTime = 1.0f / publishRate;
        InvokeRepeating("PublishVelocity", 1.0f, publishDeltaTime);
    }

    void Update() { }

    // Publish the current velocity
    private void PublishVelocity()
    {
        // Publish to ROS
        twistPublisher.PublishTwist(linearVelocity, angularVelocity);
    }
}
