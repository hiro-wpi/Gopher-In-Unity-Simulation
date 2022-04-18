using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;

/// <summary>
///     This script subscribes to twist command
///     and use robot controller to 
/// </summary>
public class AmclSubscriber : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    // Variables required for ROS communication
    public string AmclTopicName = "/amcl_pose";

    public ArticulationWheelController wheelController;
    
    void Start()
    {
        // Get ROS connection static instance
        ros = ROSConnection.GetOrCreateInstance();
        
        ros.Subscribe<PoseWithCovarianceMsg>(AmclTopicName, updatePose);
    }

    private void FixedUpdate()
    {
        // wheelController.SetRobotVelocity(targetLinearVelocity, targetAngularVelocity);
    }

    private void updatePose(PoseWithCovarianceMsg pose)
    {
        Debug.Log("in update Pose");
    }
}
