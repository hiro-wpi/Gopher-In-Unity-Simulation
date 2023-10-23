using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;

/// <summary>
///     This script publishes robot stamped twist
///     with repect to the local robot frame
/// </summary>
public class TwistStampedPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string twistStampedTopicName = "model_twist";
    [SerializeField] private string frameID = "model_twist";

    // Rigidbody
    [SerializeField] private Rigidbody rb;
    // Transform
    private Vector3 linearVelocity;
    private Vector3 angularVelocity;

    // Message
    private TwistStampedMsg twistStamped;
    // rate
    [SerializeField] private int publishRate = 10;
    private Timer timer = new Timer(10);

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistStampedMsg>(twistStampedTopicName);

        // Initialize message
        twistStamped = new TwistStampedMsg
        {
            header = new HeaderMsg(
                0, new TimeStamp(Clock.time), frameID
            )
        };

        // Rate
        timer = new Timer(publishRate);
    }

    void FixedUpdate()
    {
        timer.UpdateTimer(Time.fixedDeltaTime);

        if (timer.ShouldProcess)
        {
            PublishTwistStamped();
            timer.ShouldProcess = false;
        }
    }

    private void PublishTwistStamped()
    {
        twistStamped.header.Update();

        // Linear
        linearVelocity = rb.velocity;
        linearVelocity = rb.transform.InverseTransformDirection(linearVelocity);

        // Angular
        angularVelocity = rb.angularVelocity;
        angularVelocity = rb.transform.InverseTransformDirection(angularVelocity);
        angularVelocity = -angularVelocity;

        twistStamped.twist.linear = linearVelocity.To<FLU>();
        twistStamped.twist.angular = angularVelocity.To<FLU>();
        
        ros.Publish(twistStampedTopicName, twistStamped);
    }
}
