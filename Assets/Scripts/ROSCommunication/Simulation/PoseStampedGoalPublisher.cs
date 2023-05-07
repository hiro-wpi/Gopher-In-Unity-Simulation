using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;


/// <summary>
/// Publishes the goal to the move_base_simple/goal topic

/// </summary>
public class PoseStampedGoalPublisher : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    [SerializeField] private string poseStampedTopicName = "move_base_simple/goal";

    // MessageposeStampedTopicName
    private PoseStampedMsg poseCommand;

    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>(poseStampedTopicName);

        // Initialize message
        poseCommand = new PoseStampedMsg();
    }

    public void PublishPoseStampedCommand(Vector3 position, Vector3 rotation)
    {
        // Convert to ROS coordinate
        poseCommand.pose.position = position.To<FLU>();

        Quaternion rot = Quaternion.LookRotation(rotation, Vector3.up);
        poseCommand.pose.orientation = rot.To<FLU>();

        ros.Publish(poseStampedTopicName, poseCommand);
    }
}
