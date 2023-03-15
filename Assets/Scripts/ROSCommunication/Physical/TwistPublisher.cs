using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;


/// <summary>
///     This script publishes twist msg to a ROS topic
/// </summary>
public class TwistPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string twistTopicName = "cmd_vel";

    // Message
    private TwistMsg twist;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistMsg>(twistTopicName);

        // Initialize message
        twist = new TwistMsg();
    }

    public void PublishTwist(Vector3 linear, Vector3 angular)
    {
        // Convert to ROS coordinate
        twist.linear = linear.To<FLU>();
        twist.angular = angular.To<FLU>();

        ros.Publish(twistTopicName, twist);
    }
}
