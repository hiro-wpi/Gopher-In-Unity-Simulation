using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;


/// <summary>
/// Publishes the goal to the move_base_simple/goal topic

/// </summary>
public class AMCLPoseSubscriber : Localization
{
    
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string poseTopicName = "amcl_pose";

    // // Message poseStampedTopicName
    private PoseWithCovarianceStampedMsg poseMsg;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<PoseWithCovarianceStampedMsg>(poseTopicName, poseCallback);
    }

    void poseCallback(PoseWithCovarianceStampedMsg msg)
    {
        poseMsg = msg;
    }

    public override void UpdateLocalization()
    {
        Position = poseMsg.pose.pose.position.From<FLU>();
        Quaternion orientation = poseMsg.pose.pose.orientation.From<FLU>();
        Rotation = orientation.eulerAngles;
    }
}
