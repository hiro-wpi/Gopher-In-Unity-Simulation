using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

/// <summary>
///     This script publishes IMU messages to ROS.
/// </summary>
public class IMUPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string iMUTopicName = "base_scan";
    [SerializeField] private string iMULinkId = "laser_link";

    // Sensor
    [SerializeField] private IMU iMU;

    // Message
    private ImuMsg iMUMsg;
    // rate
    [SerializeField] private int publishRate = 10;
    private Timer timer;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<ImuMsg>(iMUTopicName);

        // Initialize messages
        iMUMsg = new();
        iMUMsg.header = new HeaderMsg(
            0, new TimeStamp(Clock.time), iMULinkId
        );

        // Use event based publishing to publish messages.
        // The publish rate is controlled by both 
        // the laser update rate, and the self publish rate.
        iMU.DataUpdatedEvent += Publish;
        timer = new Timer(publishRate);
    }

    void FixedUpdate()
    {
        timer.UpdateTimer(Time.fixedDeltaTime);
    }

    void OnDestroy()
    {
        iMU.DataUpdatedEvent -= Publish;
    }

    private void Publish()
    {
        if (!timer.ShouldProcess)
        {
            return;
        }
        timer.ShouldProcess = false;

        // Publish message
        iMUMsg.header.Update();
        iMUMsg.orientation = iMU.Orientation.To<FLU>();
        iMUMsg.angular_velocity = iMU.AngularVelocity.To<FLU>();
        iMUMsg.linear_acceleration = iMU.LinearAcceleration.To<FLU>();

        ros.Publish(iMUTopicName, iMUMsg);
    }
}
