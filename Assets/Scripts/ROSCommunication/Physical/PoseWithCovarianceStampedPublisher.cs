using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.Core;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Std;
using RosMessageTypes.Geometry;

/// <summary>
///     Publishes the initial pose to the ROS AMCL node
/// </summary>
public class PoseWithCovarianceStampedPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string poseTopicName = "initcialpose";
    [SerializeField] private string frameID = "map";

    // PoseWithCovarianceStampedMsg
    private PoseWithCovarianceStampedMsg poseCommand;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseWithCovarianceStampedMsg>(poseTopicName);

        // Initialize message
        poseCommand = new PoseWithCovarianceStampedMsg(
            new HeaderMsg(
                Clock.GetCount(), new TimeStamp(Clock.time), frameID
            ),
            new PoseWithCovarianceMsg(
                new PoseMsg(
                    new PointMsg(0, 0, 0),
                    new QuaternionMsg(0, 0, 0, 1)
                ),
                new double[36] { 
                    0.25, 0, 0, 0, 0, 0,
                    0, 0.25, 0, 0, 0, 0,
                    0, 0,    0, 0, 0, 0,
                    0, 0,    0, 0, 0, 0,
                    0, 0,    0, 0, 0, 0,
                    0, 0,    0, 0, 0, 0.06853892326654787 
                }
            )
        );
    }

    public void PublishPoseStampedCommand(Vector3 position, Vector3 rotation)
    {
        // Publish message
        poseCommand.header = new HeaderMsg(
            Clock.GetCount(), new TimeStamp(Clock.time), frameID
        );

        // Convert to ROS coordinate
        poseCommand.pose.pose.position = position.To<FLU>();
        poseCommand.pose.pose.orientation = Quaternion.Euler(rotation).To<FLU>();

        // Publish message
        ros.Publish(poseTopicName, poseCommand);
    }
}
