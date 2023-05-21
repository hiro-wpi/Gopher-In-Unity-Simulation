using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;


/// <summary>
/// Publishes the iniitcial pose to the AMCL node

/// </summary>
public class PoseWithCovarianceStampedPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string poseWithCovarianceStampedTopicName = "initcialpose";

    // PoseWithCovarianceStampedMsg
    private PoseWithCovarianceStampedMsg poseCommand;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseWithCovarianceStampedMsg>(poseWithCovarianceStampedTopicName);

        // Initialize message
        poseCommand = new PoseWithCovarianceStampedMsg();
    }

    public void PublishPoseStampedCommand(Vector3 position, Vector3 rotation)
    {
        // Convert to ROS coordinate
        poseCommand.pose.pose.position = position.To<FLU>();

        Quaternion rot = Quaternion.LookRotation(rotation, Vector3.up);
        poseCommand.pose.pose.orientation = rot.To<FLU>();

        // Set up the covariance matrix
        poseCommand.pose.covariance = new double[36] { 0.25, 0, 0, 0, 0, 0,
                                                       0, 0.25, 0, 0, 0, 0,
                                                       0, 0, 0.25, 0, 0, 0,
                                                       0, 0, 0, 0.25, 0, 0,
                                                       0, 0, 0, 0, 0.25, 0,
                                                       0, 0, 0, 0, 0, 0.25 };

        // Publish message
        ros.Publish(poseWithCovarianceStampedTopicName, poseCommand);
    }
}
