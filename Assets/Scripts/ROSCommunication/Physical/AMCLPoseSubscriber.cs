using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;

/// <summary>
///     This script subscribes to AMCL result pose
/// </summary>
public class AMCLPoseSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string poseTopicName = "amcl_pose";

    // Message
    private Vector3 position;
    private Vector3 rotation;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PoseWithCovarianceStampedMsg>(poseTopicName, poseCallback);
    }

    void poseCallback(PoseWithCovarianceStampedMsg poseMsg)
    {
        position = poseMsg.pose.pose.position.From<FLU>();
        rotation = poseMsg.pose.pose.orientation.From<FLU>().eulerAngles;
    }

    public (Vector3, Vector3) GetPose()
    {
        return (position, rotation);
    }
}
