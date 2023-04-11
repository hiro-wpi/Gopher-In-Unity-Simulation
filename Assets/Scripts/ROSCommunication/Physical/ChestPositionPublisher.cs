using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.GopherRosClearcore;

/// <summary>
///     This script publishes twist msg to a ROS topic
/// </summary>
public class ChestPositionPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string positionTopicName = "cmd_vel";

    // Message
    private PositionMsg positionMsg;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PositionMsg>(positionTopicName);

        // Initialize message
        positionMsg = new PositionMsg();
    }

    public void PublishTwist(double position, double velocity)
    {
        // Convert to ROS coordinate
        positionMsg.position = position;
        positionMsg.velocity = velocity;

        ros.Publish(positionTopicName, positionMsg);
    }
}
