using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.KortexDriver;
using GeoTwist = RosMessageTypes.Geometry.TwistMsg;
using KortexTwist = RosMessageTypes.KortexDriver.TwistMsg;


/// <summary>
///     This script publishes twistCommand command msg to a ROS topic.
///     TwistCommand is a customized message for Kinova arm control.
/// </summary>
public class TwistCommandPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string twistCommandTopicName = "cartesian_velocity";

    // Message
    private TwistCommandMsg twistCommand;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TwistCommandMsg>(twistCommandTopicName);

        // Initialize message
        twistCommand = new TwistCommandMsg
        {
            reference_frame = 0,
            twist = new KortexTwist(),
            duration = 0
        };
    }

    public void PublishTwistCommand(Vector3 linear, Vector3 angular)
    {
        // Convert to ROS coordinate
        GeoTwist twist = new GeoTwist
        {
            linear = linear.To<FLU>(),
            angular = angular.To<FLU>()
        };

        // Convert to kortex type
        twistCommand.twist.linear_x = (float)twist.linear.x;
        twistCommand.twist.linear_y = (float)twist.linear.y;
        twistCommand.twist.linear_z = (float)twist.linear.z;
        twistCommand.twist.angular_x = (float)twist.angular.x;
        twistCommand.twist.angular_y = (float)twist.angular.y;
        twistCommand.twist.angular_z = (float)twist.angular.z;

        ros.Publish(twistCommandTopicName, twistCommand);
    }
}
